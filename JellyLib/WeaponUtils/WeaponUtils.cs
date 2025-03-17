using System;
using Lua.Proxy;
using Lua;

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
            if (!IsMountedWeapon(weapon))
            {
                return null;
            }
            return new MountedWeaponProxy(weapon as MountedWeapon);
        }
    }
}

