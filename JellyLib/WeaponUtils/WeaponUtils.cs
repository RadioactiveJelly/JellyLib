using System;
using System.Collections.Generic;
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
            var modId = weaponEntry.sourceMod.workshopItemId.m_PublishedFileId;
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
}

