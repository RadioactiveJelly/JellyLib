using System;
using Lua;
using Lua.Proxy;
using MoonSharp.Interpreter;

namespace JellyLib.WeaponUtils
{
    [Proxy(typeof(WeaponUtils))]
    public class WeaponUtilsProxy : IProxy
    {

        public static bool IsThrowableWeapon(WeaponProxy weaponProxy)
        {
            return WeaponUtils.IsThrowableWeapon(weaponProxy._value);
        }

        public static bool IsMountedWeapon(WeaponProxy weaponProxy)
        {
            return WeaponUtils.IsMountedWeapon(weaponProxy._value);
        }

        //If the correct proxy is already being used return true.
        public static bool IsMountedWeapon(MountedWeaponProxy weaponProxy)
        {
            return true;
        }

        public static MountedWeaponProxy AsMountedWeapon(WeaponProxy weaponProxy)
        {
            return WeaponUtils.AsMountedWeapon(weaponProxy._value);
        }

        public static MountedWeaponProxy AsMountedWeapon(MountedWeaponProxy weaponProxy)
        {
            return weaponProxy;
        }

        public static void AddWeaponOverride(WeaponEntryProxy weaponEntryProxy, WeaponOverrideProxy weaponOverrideProxy)
        {
            if (weaponEntryProxy._value == null)
            {
                throw new ScriptRuntimeException("No weapon entry provided!");
            }
            WeaponUtils.OverrideManager.AddWeaponOverride(weaponEntryProxy._value, weaponOverrideProxy._value);
        }

        public static void RemoveWeaponOverride(WeaponEntryProxy weaponEntryProxy)
        {
            if (weaponEntryProxy._value == null)
            {
                throw new ScriptRuntimeException("No weapon entry provided!");
            }

            WeaponUtils.OverrideManager.RemoveWeaponOverride(weaponEntryProxy._value);
        }

        [MoonSharpHidden]
        public object GetValue()
        {
            throw new InvalidOperationException("Proxied type is static.");
        }
    }
}

