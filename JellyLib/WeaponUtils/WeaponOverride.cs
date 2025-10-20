namespace JellyLib.WeaponUtils
{
    public struct WeaponOverride
    {
        //Ammo
        public int? maxAmmo;
        public int? maxSpareAmmo;
        public int? maxAmmoPerReload;
        public int? resupplyNumber;
        
        //Damage
        public int? damage;
        public int? balanceDamage;
        
        //Recoil
        public float? kickback;
        public float? randomKick;
        public float? snapMagnitude;
        public float? snapDuration;
        public float? snapFrequency;
        
        //Spread
        public float? spread;
        public float? followupSpreadGain;
        public float? followupMaxSpreadHip;
        public float? followupMaxSpreadAim;
        public float? followupSpreadStayTime;
        public float? followupSpreadDissipateTime;

        public float? spreadProneMultiplier;
        public float? followupSpreadProneMultiplier;
        
        public float? cooldown;
        public bool autoAdjustAllowedReloads;
    }
}
