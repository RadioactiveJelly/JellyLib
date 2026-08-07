using System;
using Lua;
using Lua.Proxy;
using MoonSharp.Interpreter;
using JellyLib.Utilities;

namespace JellyLib.WeaponUtils
{
    [Proxy(typeof(WeaponUtils))]
    public class WeaponUtilsProxy : IProxy
    {
        public static bool IsThrowableWeapon(WeaponProxy weaponProxy)
        {
            if (weaponProxy._value == null)
            {
                throw new ScriptRuntimeException("weapon cannot be null");
            }
            return WeaponUtils.IsThrowableWeapon(weaponProxy._value);
        }

        public static bool IsMountedWeapon(WeaponProxy weaponProxy)
        {
            if (weaponProxy._value == null)
            {
                throw new ScriptRuntimeException("weapon cannot be null");
            }
            return WeaponUtils.IsMountedWeapon(weaponProxy._value);
        }

        //If the correct proxy is already being used return true.
        public static bool IsMountedWeapon(MountedWeaponProxy weaponProxy)
        {
            if (weaponProxy._value == null)
            {
                throw new ScriptRuntimeException("weapon cannot be null");
            }
            return true;
        }

        public static MountedWeaponProxy AsMountedWeapon(WeaponProxy weaponProxy)
        {
            if (weaponProxy._value == null)
            {
                throw new ScriptRuntimeException("weapon cannot be null");
            }
            return WeaponUtils.AsMountedWeapon(weaponProxy._value);
        }

        public static MountedWeaponProxy AsMountedWeapon(MountedWeaponProxy weaponProxy)
        {
            if (weaponProxy._value == null)
            {
                throw new ScriptRuntimeException("weapon cannot be null");
            }
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

        public static void AddWeaponInstanceOverride(WeaponProxy weaponProxy, WeaponOverrideProxy weaponOverrideProxy)
        {
            if (weaponProxy._value == null)
            {
                throw new ScriptRuntimeException("No weapon provided!");
            }
            
            WeaponUtils.OverrideManager.AddWeaponInstanceOverride(weaponProxy._value, weaponOverrideProxy._value);
        }

        public static void AddWeaponInstanceOverride(MountedWeaponProxy mountedWeaponProxy, WeaponOverrideProxy weaponOverrideProxy)
        {
            if (mountedWeaponProxy._value == null)
            {
                throw new ScriptRuntimeException("No weapon provided!");
            }
            
            WeaponUtils.OverrideManager.AddWeaponInstanceOverride(mountedWeaponProxy._value, weaponOverrideProxy._value);
        }

        public static void RemoveWeaponInstanceOverride(WeaponProxy weaponProxy)
        {
            if (weaponProxy._value == null)
            {
                throw new ScriptRuntimeException("No weapon provided!");
            }
            
            WeaponUtils.OverrideManager.RemoveWeaponInstanceOverride(weaponProxy._value);
        }
        
        public static void RemoveWeaponInstanceOverride(MountedWeaponProxy mountedWeaponProxy)
        {
            if (mountedWeaponProxy._value == null)
            {
                throw new ScriptRuntimeException("No weapon provided!");
            }
            
            WeaponUtils.OverrideManager.RemoveWeaponInstanceOverride(mountedWeaponProxy._value);
        }

        public static WeaponOverrideProxy GetWeaponInstanceOverride(WeaponProxy weaponProxy)
        {
            if (weaponProxy._value == null)
            {
                throw new ScriptRuntimeException("No weapon provided!");
            }

            if (!WeaponUtils.OverrideManager.GetInstanceOverride(weaponProxy._value, out var weaponOverride))
                return null;
            
            var proxy = new WeaponOverrideProxy(weaponOverride);
            return proxy;
        }

        public static WeaponManager.WeaponEntry GetWeaponEntry(string weaponEntryName, ulong modId)
        {
            var weaponEntry = WeaponUtils.GetWeaponEntry(weaponEntryName, modId);
            return weaponEntry;
            //return weaponEntry == null ? null : new WeaponEntryProxy(weaponEntry);
        }

        public static float GetWeaponDamage(WeaponProxy weaponProxy)
        {
            if (weaponProxy._value == null)
            {
                throw new ScriptRuntimeException("No weapon provided!");
            }
            
            return WeaponUtils.GetWeaponDamage(weaponProxy._value);
        }

        public static float GetWeaponDamage(MountedWeaponProxy mountedWeaponProxy)
        {
            if (mountedWeaponProxy._value == null)
            {
                throw new ScriptRuntimeException("No weapon provided!");
            }
            
            return WeaponUtils.GetWeaponDamage(mountedWeaponProxy._value);
        }

        public static float GetWeaponExplosionDamage(WeaponProxy weaponProxy)
        {
            if (weaponProxy._value == null)
            {
                throw new ScriptRuntimeException("No weapon provided!");
            }

            return WeaponUtils.GetWeaponExplosionDamage(weaponProxy._value);
        }
        
        public static float GetWeaponExplosionDamage(MountedWeaponProxy mountedWeaponProxy)
        {
            if (mountedWeaponProxy._value == null)
            {
                throw new ScriptRuntimeException("No weapon provided!");
            }

            return WeaponUtils.GetWeaponExplosionDamage(mountedWeaponProxy._value);
        }

        public static void SetWeaponCustomData(WeaponProxy weaponProxy, CustomDataProxy customDataProxy)
        {
            if (weaponProxy._value == null)
                throw new ScriptRuntimeException("No weapon provided!");
            if (customDataProxy._value == null)
                throw new ScriptRuntimeException("No custom data provided!");
            
            WeaponUtils.CustomDataManager.AddWeaponCustomData(weaponProxy._value, customDataProxy._value);
        }

        public static CustomDataProxy GetWeaponCustomData(WeaponProxy weaponProxy)
        {
            if (weaponProxy._value == null)
                throw new ScriptRuntimeException("No weapon provided!");

            return WeaponUtils.CustomDataManager.TryGetWeaponCustomData(weaponProxy._value, out var data) ? new CustomDataProxy(data) : null;
        }
        
        [MoonSharpHidden]
        public object GetValue()
        {
            throw new InvalidOperationException("Proxied type is static.");
        }
    }
}

