using System.Collections.Generic;

namespace JellyLib.WeaponUtils
{
    public class WeaponOverrideManager
    {
        /// <summary>
        /// Dictionary containing overrides applied to all instances of a weapon entry.
        /// </summary>
        private readonly Dictionary<WeaponManager.WeaponEntry, WeaponOverride> _weaponOverrides = new();
        
        /// <summary>
        /// Dictionary containing overrides applied to one instance of a weapon. Will take priority over any global overrides.
        /// </summary>
        private readonly Dictionary<Weapon, WeaponOverride> _instanceOverrides = new();
        
        /// <summary>
        /// Dictionary containing overrides for an instance of a projectile. Overrides are removed when a projectile is pooled.
        /// </summary>
        private readonly Dictionary<Projectile, ProjectileOverride> _projectileOverrides = new();

        public void Clear()
        {
            _weaponOverrides.Clear();
            _instanceOverrides.Clear();
        }

        public void AddWeaponOverride(WeaponManager.WeaponEntry weaponEntry, WeaponOverride weaponOverride)
        {
            var modId = weaponEntry.sourceMod.workshopItemId.m_PublishedFileId;
            _weaponOverrides[weaponEntry] = weaponOverride;
        }

        public void RemoveWeaponOverride(WeaponManager.WeaponEntry weaponEntry)
        {
            var modId = weaponEntry.sourceMod.workshopItemId.m_PublishedFileId;
            _weaponOverrides.Remove(weaponEntry);
        }

        public void AddWeaponInstanceOverride(Weapon weaponInstance, WeaponOverride weaponOverride)
        {
            _instanceOverrides[weaponInstance] = weaponOverride;
            ApplyOverrides(weaponInstance);
        }

        public void RemoveWeaponInstanceOverride(Weapon weaponInstance)
        {
            _instanceOverrides.Remove(weaponInstance);
            ApplyOverrides(weaponInstance);
        }

        public bool GetWeaponOverride(WeaponManager.WeaponEntry weaponEntry, out WeaponOverride weaponOverride)
        {
            if (weaponEntry != null) return _weaponOverrides.TryGetValue(weaponEntry, out weaponOverride);
            
            weaponOverride = default;
            return false;
        }

        public bool GetInstanceOverride(Weapon weapon, out WeaponOverride weaponOverride)
        {
            return _instanceOverrides.TryGetValue(weapon, out weaponOverride);
        }

        public static void ApplyOverrides(Weapon weapon)
        {
            //Apply global overrides first.
            if (WeaponUtils.OverrideManager.GetWeaponOverride(weapon.weaponEntry, out var globalOverride))
                ApplyOverride(weapon, globalOverride);
            
            //Apply instance overrides after.
            if (WeaponUtils.OverrideManager.GetInstanceOverride(weapon, out var instanceOverride))
                ApplyOverride(weapon, instanceOverride);
        }

        public static void ApplyOverride(Weapon weapon, WeaponOverride weaponOverride)
        {
            if (weaponOverride.maxAmmo.HasValue)
            {
                weapon.configuration.ammo = weaponOverride.maxAmmo.Value;
                weapon.ammo = weapon.configuration.ammo;
            }
            if (weaponOverride.maxSpareAmmo.HasValue)
            {
                weapon.configuration.spareAmmo = weaponOverride.maxSpareAmmo.Value;
                weapon.spareAmmo = weapon.configuration.spareAmmo;
            }
            
            weapon.configuration.resupplyNumber = weaponOverride.resupplyNumber ?? weapon.configuration.resupplyNumber;
            weapon.configuration.maxAmmoPerReload = weaponOverride.maxAmmoPerReload ?? weapon.configuration.maxAmmoPerReload;

            if (weaponOverride.autoAdjustAllowedReloads)
            {
                List<int> allowedReloads = new List<int>();
                for (var i = 0; i < weapon.configuration.ammo; i++)
                {
                    allowedReloads.Add(i+1);
                }
                weapon.configuration.allowedReloads = allowedReloads.ToArray();
            }
            
            weapon.configuration.kickback = weaponOverride.kickback ?? weapon.configuration.kickback;
            weapon.configuration.randomKick = weaponOverride.randomKick ?? weapon.configuration.randomKick;
            weapon.configuration.snapMagnitude = weaponOverride.snapMagnitude ?? weapon.configuration.snapMagnitude;
            weapon.configuration.snapDuration = weaponOverride.snapDuration ?? weapon.configuration.snapDuration;
            weapon.configuration.snapFrequency = weaponOverride.snapFrequency ?? weapon.configuration.snapFrequency;
            
            weapon.configuration.spread = weaponOverride.spread ?? weapon.configuration.spread;
            weapon.configuration.followupSpreadGain = weaponOverride.followupSpreadGain ?? weapon.configuration.followupSpreadGain;
            weapon.configuration.followupMaxSpreadHip = weaponOverride.followupMaxSpreadHip ?? weapon.configuration.followupMaxSpreadHip;
            weapon.configuration.followupMaxSpreadAim = weaponOverride.followupMaxSpreadAim ?? weapon.configuration.followupMaxSpreadAim;
            weapon.configuration.followupSpreadStayTime = weaponOverride.followupSpreadStayTime ?? weapon.configuration.followupSpreadStayTime;
            weapon.configuration.followupSpreadDissipateTime = weaponOverride.followupSpreadDissipateTime ?? weapon.configuration.followupSpreadDissipateTime;
            weapon.configuration.spreadProneMultiplier = weaponOverride.spreadProneMultiplier ?? weapon.configuration.spreadProneMultiplier;
            weapon.configuration.followupSpreadProneMultiplier = weaponOverride.followupSpreadProneMultiplier ?? weapon.configuration.followupSpreadProneMultiplier;
            
            weapon.configuration.cooldown = weaponOverride.cooldown ?? weapon.configuration.cooldown;
        }

        public void SetProjectileOverride(Projectile projectile, ProjectileOverride projectileOverride)
        {
            _projectileOverrides[projectile] = projectileOverride;
        }

        public bool TryGetProjectileOverride(Projectile projectile, out ProjectileOverride projectileOverride)
        {
            if(projectile != null) return _projectileOverrides.TryGetValue(projectile, out projectileOverride);
            
            projectileOverride = default;
            return false;
        }

        public void RemoveProjectileOverride(Projectile projectile)
        {
            _projectileOverrides.Remove(projectile);
        }
    }
}

