using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Lua.Proxy;
using Lua;
using Steamworks;
using HarmonyLib;
using Lua.Wrapper;
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
        private static Dictionary<ulong, string> _modNamesById = new();

        private static bool _doneLoading;
        public static bool  DoneLoading => _doneLoading;

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
                var weaponEntryName = weaponEntry.name.Trim();
                if (_weaponsByModId.TryGetValue(modId, out var weaponSet))
                {
                    if (weaponSet.ContainsKey(weaponEntryName))
                    {
                        var alternateName = $"{weaponEntryName}({weaponEntry.slot})";
                        weaponSet.Add(alternateName, weaponEntry);
                        Plugin.Logger.LogWarning($"[{nameof(WeaponUtils)}.{nameof(SortWeaponEntriesByModId)}] Weapon Entry with name {weaponEntryName} in group {modId} already found. Registering with alternate name {alternateName} instead.");
                        continue;
                    }
                    weaponSet.Add(weaponEntryName, weaponEntry);
                }
                else
                {
                    var newSet = new Dictionary<string, WeaponManager.WeaponEntry>(StringComparer.InvariantCultureIgnoreCase)
                    {
                        [weaponEntryName] = weaponEntry
                    };
                    _weaponsByModId.Add(modId, newSet);
                    _modNamesById.Add(modId, weaponEntry.sourceMod.title);
                }
                
                //Plugin.Logger.LogInfo($"[{nameof(WeaponUtils)}.{nameof(SortWeaponEntriesByModId)}] Registered: {weaponEntry.name} to group {modId}");
            }
            stopwatch.Stop();
            Plugin.Logger.LogInfo($"[{nameof(WeaponUtils)}.{nameof(SortWeaponEntriesByModId)}] Operation took {stopwatch.ElapsedMilliseconds}ms.");
            _doneLoading = true;
        }

        public static WeaponManager.WeaponEntry GetWeaponEntry(string weaponEntryName, ulong modId)
        {
            var key = _weaponsByModId.ContainsKey(modId) ? modId : 0;
            if (!_weaponsByModId.TryGetValue(key, out var weaponSet))
                return null;
            
            var trimmedString = weaponEntryName.Trim();
            return !weaponSet.TryGetValue(trimmedString, out var weaponEntry) ? null : weaponEntry;
        }

        public static void DumpWeaponNames()
        {
            Directory.CreateDirectory($@"{Plugin.filePath}\dumps\");
            
            var dump = "";
            foreach (var kvp in _weaponsByModId)
            { 
                if (kvp.Key == 0)
                    dump += "Vanilla or RFToolsExport\n";
                else
                    dump += $"Mod {_modNamesById[kvp.Key]} (ID: {kvp.Key}): \n";
                
                foreach (var weaponName in kvp.Value.Keys)
                {
                    dump += "-" + weaponName + "\n";
                    var weaponEntry = kvp.Value[weaponName];
                    dump += "--Tags: \n";
                    foreach (var tag in weaponEntry.tags) 
                        dump += $"----{tag}\n";
                    if (!weaponEntry.prefab)
                        continue;
                    var weapon = weaponEntry.prefab.GetComponent<Weapon>();
                    if (!weapon)
                        continue;
                    dump += "--Stats: \n";
                    dump += $"----Max Ammo: {weapon.configuration.ammo}\n";
                    dump += $"----Max Spare Ammo: {weapon.configuration.spareAmmo}\n";
                    dump += $"----Kickback: {weapon.configuration.kickback}\n";
                    dump += $"----Random Kick: {weapon.configuration.randomKick}\n";
                    dump += $"----Snap Magnitude: {weapon.configuration.snapMagnitude}\n";
                    dump += $"----Snap Duration: {weapon.configuration.snapDuration}\n";
                    dump += $"----Snap Frequency: {weapon.configuration.snapFrequency}\n";
                    dump += $"----Cooldown: {weapon.configuration.cooldown}\n";
                    var projectilePrefab = weapon.configuration.projectilePrefab;
                    if (!projectilePrefab)
                        continue;
                    var projectile = projectilePrefab.GetComponent<Projectile>();
                    if (!projectile)
                        continue;
                    dump += $"----Health Damage: {projectile.configuration.damage}\n";
                    dump += $"----Balance Damage: {projectile.configuration.balanceDamage}\n";
                    dump += $"----Projectile Speed: {projectile.configuration.speed}\n";
                    dump += $"----Projectile Drop-Off End: {projectile.configuration.dropoffEnd}\n";
                }
                File.WriteAllText($@"{Plugin.filePath}\dumps\{kvp.Key}.txt", dump);
                dump = "";
            }
            
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

            if (weaponOverride.kickback.HasValue)
                __result.configuration.kickback = weaponOverride.kickback.Value;
            
            if (weaponOverride.randomKick.HasValue)
                __result.configuration.kickback = weaponOverride.randomKick.Value;
            
            if (weaponOverride.snapMagnitude.HasValue)
                __result.configuration.kickback = weaponOverride.snapMagnitude.Value;
            
            if (weaponOverride.snapDuration.HasValue)
                __result.configuration.kickback = weaponOverride.snapDuration.Value;
            
            if (weaponOverride.snapFrequency.HasValue)
                __result.configuration.kickback = weaponOverride.snapFrequency.Value;
            
            if(weaponOverride.cooldown.HasValue)
                __result.configuration.cooldown = weaponOverride.cooldown.Value;
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

    [HarmonyPatch(typeof(Projectile), nameof(Projectile.Damage))]
    public class PatchProjectileDamage
    {
        static bool Prefix(Projectile __instance, ref float __result)
        {
            if (__instance == null) return false;
            if (__instance.sourceWeapon == null) return true;
            if (__instance.sourceWeapon.weaponEntry == null) return true;
            
            var hasOverride = WeaponUtils.OverrideManager.GetWeaponOverride(__instance.sourceWeapon.weaponEntry, out var weaponOverride);
            if (!hasOverride) return true;

            if (!weaponOverride.damage.HasValue)
                return true;
            
            var dropOff = __instance.configuration.damageDropOff.Evaluate(__instance.travelDistance / __instance.configuration.dropoffEnd);
            
            __result = dropOff * weaponOverride.damage.Value;
            
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.BalanceDamage))]
    public class PatchProjectileBalanceDamage
    {
        static bool Prefix(Projectile __instance, ref float __result)
        {
            if (__instance == null) return false;
            if (__instance.sourceWeapon == null) return true;
            if (__instance.sourceWeapon.weaponEntry == null) return true;
            
            var hasOverride = WeaponUtils.OverrideManager.GetWeaponOverride(__instance.sourceWeapon.weaponEntry, out var weaponOverride);
            if (!hasOverride) return true;

            if (!weaponOverride.balanceDamage.HasValue)
                return true;
            
            var dropOff = __instance.configuration.damageDropOff.Evaluate(__instance.travelDistance / __instance.configuration.dropoffEnd);
            __result = dropOff * weaponOverride.balanceDamage.Value;
            
            return false;
        }
    }
}

