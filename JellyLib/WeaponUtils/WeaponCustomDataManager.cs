using System.Collections.Generic;
using JellyLib.Utilities;

namespace JellyLib.WeaponUtils
{
    public class WeaponCustomDataManager
    {
        /// <summary>
        /// Dictionary containing custom data associated to an instance of a weapon.
        /// Custom data is removed when a weapon is destroyed.
        /// </summary>
        private readonly Dictionary<Weapon, CustomData> _weaponInstanceCustomData = new();

        public void Clear()
        {
            _weaponInstanceCustomData.Clear();
        }

        public void AddWeaponCustomData(Weapon weapon, CustomData data)
        {
            //Set data to be immutable so that accessing it via RS after its become associated with something means it can no longer be changed.
            data.SetImmutable();
            _weaponInstanceCustomData[weapon] = data;
        }

        public bool TryGetWeaponCustomData(Weapon weapon, out CustomData data)
        {
            return _weaponInstanceCustomData.TryGetValue(weapon, out data);
        }

        public void ClearWeaponData(Weapon weapon)
        {
            _weaponInstanceCustomData.Remove(weapon);
        }
    }
}

