using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lua.Proxy;
using Lua;
using Steamworks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Windows.WebCam;

namespace JellyLib.WeaponUtils
{
    public static class WeaponUtils
    {
        public static bool IsThrowableWeapon(Weapon weapon)
        {
            return weapon is ThrowableWeapon;
        }

        public static bool IsMountedWeapon(Weapon weapon)
        {
            return weapon is MountedWeapon;
        }

        //Returns null if weapon passed is not a MountedWeapon
        public static MountedWeaponProxy AsMountedWeapon(Weapon weapon)
        {
            var mountedWeapon = weapon as MountedWeapon;
            return !mountedWeapon ? null : new MountedWeaponProxy(mountedWeapon);
        }

        private static WeaponOverrideManager _overrideManager; 
        public static WeaponOverrideManager OverrideManager => _overrideManager ??= new WeaponOverrideManager();

        private static Dictionary<ulong, Dictionary<string, WeaponManager.WeaponEntry>> _weaponsByModId;

        public static void SortWeaponEntriesByModId()
        {
            if (_weaponsByModId == null)
                _weaponsByModId = new();
            else
                return;
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach(var weaponEntry in WeaponManager.instance.allWeapons)
            {
                var modId = weaponEntry.sourceMod.workshopItemId.m_PublishedFileId;
                if (_weaponsByModId.TryGetValue(modId, out var weaponSet))
                {
                    if (weaponSet.ContainsKey(weaponEntry.name))
                    {
                        var alternateName = $"{weaponEntry.name}({weaponEntry.slot})";
                        weaponSet.Add(alternateName, weaponEntry);
                        Plugin.Logger.LogWarning($"[{nameof(WeaponUtils)}.{nameof(SortWeaponEntriesByModId)}] Weapon Entry with name {weaponEntry.name} in group {modId} already found. Registering with alternate name {alternateName} instead.");
                        continue;
                    }
                    weaponSet.Add(weaponEntry.name, weaponEntry);
                }
                else
                {
                    var newSet = new Dictionary<string, WeaponManager.WeaponEntry>(StringComparer.InvariantCultureIgnoreCase)
                    {
                        [weaponEntry.name] = weaponEntry
                    };
                    _weaponsByModId.Add(modId, newSet);
                }
                
                //Plugin.Logger.LogInfo($"[{nameof(WeaponUtils)}.{nameof(SortWeaponEntriesByModId)}] Registered: {weaponEntry.name} to group {modId}");
            }
            stopwatch.Stop();
            Plugin.Logger.LogInfo($"[{nameof(WeaponUtils)}.{nameof(SortWeaponEntriesByModId)}] Operation took {stopwatch.ElapsedMilliseconds}ms.");
        }

        public static WeaponManager.WeaponEntry GetWeaponEntry(string weaponEntryName, ulong modId)
        {
            if (!_weaponsByModId.TryGetValue(modId, out var weaponSet))
                return null;
            
            return !weaponSet.TryGetValue(weaponEntryName, out var weaponEntry) ? null : weaponEntry;
        }
    }

    public class WeaponOverrideManager
    {
        private readonly Dictionary<WeaponManager.WeaponEntry, WeaponOverride> _weaponOverrides = new();

        public void Clear()
        {
            _weaponOverrides.Clear();
        }
        
        public void AddWeaponOverride(WeaponManager.WeaponEntry weaponEntry, WeaponOverride weaponOverride)
        {
            var modId = weaponEntry.sourceMod.workshopItemId.m_PublishedFileId;
            _weaponOverrides[weaponEntry] = weaponOverride;
            Plugin.Logger.LogInfo($"Registered override for {weaponEntry.name} (mod ID: {modId})");
        }

        public void RemoveWeaponOverride(WeaponManager.WeaponEntry weaponEntry)
        {
            var modId = weaponEntry.sourceMod.workshopItemId.m_PublishedFileId;
            _weaponOverrides.Remove(weaponEntry);
            Plugin.Logger.LogInfo($"Removed override for {weaponEntry.name} (mod ID: {modId})");
        }

        public bool GetWeaponOverride(WeaponManager.WeaponEntry weaponEntry, out WeaponOverride weaponOverride)
        {
            return _weaponOverrides.TryGetValue(weaponEntry, out weaponOverride);
        }
    }
    
    [HarmonyPatch(typeof(Actor), "SpawnWeapon")]
    public class WeaponAwakePatch
    {
        static void Postfix(Weapon __result)
        {
            if (__result == null) return;
            if (__result.weaponEntry == null) return;
            
            var hasOverride = WeaponUtils.OverrideManager.GetWeaponOverride(__result.weaponEntry, out var weaponOverride);
            if (!hasOverride) return;

            if (weaponOverride.maxAmmo.HasValue)
            {
                __result.configuration.ammo = weaponOverride.maxAmmo.Value;
                __result.ammo = __result.configuration.ammo;
            }
            if (weaponOverride.maxSpareAmmo.HasValue)
            {
                __result.configuration.spareAmmo = weaponOverride.maxSpareAmmo.Value;
                __result.spareAmmo = __result.configuration.spareAmmo;
            }

            if (weaponOverride.maxAmmoPerReload.HasValue)
            {
                __result.configuration.maxAmmoPerReload = weaponOverride.maxAmmoPerReload.Value;
            }

            if (weaponOverride.autoAdjustAllowedReloads)
            {
                List<int> allowedReloads = new List<int>();
                for (var i = 0; i < __result.configuration.ammo; i++)
                {
                    allowedReloads.Add(i+1);
                }
                __result.configuration.allowedReloads = allowedReloads.ToArray();
            }
        }
    }
    
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.ReturnToMenu))]
    public class PatchReturnToMenu
    {
        static bool Prefix(GameManager __instance)
        {
            WeaponUtils.OverrideManager.Clear();
            Plugin.Logger.LogInfo($"{nameof(WeaponUtils)}.{nameof(GameManager.ReturnToMenu)}.Prefix: Cleared weapon overrides.");
            return true;
        }
    }
    
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.RestartLevel))]
    public class PatchRestartLevel
    {
        static bool Prefix(GameManager __instance)
        {
            WeaponUtils.OverrideManager.Clear();
            Plugin.Logger.LogInfo($"{nameof(WeaponUtils)}.{nameof(GameManager.RestartLevel)}.Prefix: Cleared weapon overrides.");
            return true;
        }
    }
    
    [HarmonyPatch(typeof(ModManager), "FinalizeLoadedModContent")]
    public class PatchFinalizeLoadedModContent
    {
        static void Postfix()
        {
            WeaponUtils.SortWeaponEntriesByModId();
        }
    }
}

