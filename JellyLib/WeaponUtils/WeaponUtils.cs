using System;
using System.Collections.Generic;
using Lua.Proxy;
using Lua;
using Steamworks;
using HarmonyLib;
using UnityEngine;

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
        private readonly Dictionary<ulong, Dictionary<WeaponManager.WeaponEntry, WeaponOverride>> _weaponOverrides = new();

        public void AddWeaponOverride(WeaponManager.WeaponEntry weaponEntry, WeaponOverride weaponOverride)
        {
            var modId = weaponEntry.sourceMod.workshopItemId.m_PublishedFileId;
            if (_weaponOverrides.TryGetValue(modId, out var modSet))
            {
                modSet[weaponEntry] = weaponOverride;
            }
            else
            {
                var newModSet = new Dictionary<WeaponManager.WeaponEntry, WeaponOverride>();
                newModSet[weaponEntry] = weaponOverride;
                _weaponOverrides[modId] = newModSet;
            }
            Plugin.Logger.LogInfo($"Registered override for {weaponEntry.name} ({modId})");
        }

        public void RemoveWeaponOverride(WeaponManager.WeaponEntry weaponEntry)
        {
            var modId = weaponEntry.sourceMod.workshopItemId.m_PublishedFileId;
            if (_weaponOverrides.TryGetValue(modId, out var modSet))
            {
                modSet.Remove(weaponEntry);
                Plugin.Logger.LogInfo($"Removed override for {weaponEntry.name} ({modId})");
            }
        }
    }
    
    

    [HarmonyPatch(typeof(Actor), "SpawnWeapon")]
    public class WeaponAwakePatch
    {
        static void Postfix(Weapon __result)
        {
            if(__result == null) return;

            if (__result.weaponEntry.sourceMod.isOfficialContent)
                return;
            
            Plugin.Logger.LogInfo($"{__result.name } from {__result.weaponEntry.sourceMod.workshopItemId.m_PublishedFileId} has been loaded");
        }
    }
}

