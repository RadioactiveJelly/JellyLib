namespace JellyLib.DamageSystem
{
    /// <summary>
    /// A struct for use in damage calculation.
    /// </summary>
    /// <param name="healthDamageMultiplier">The value health damage will be multiplied by.</param>
    /// <param name="balanceDamageMultiplier">The value balance damage will be multiplied by.</param>
    /// <param name="flatHealthDamageModifier">The value health damage will be reduced by.</param>
    /// <param name="flatBalanceDamageModifier">The value balance damage will be reduced by.</param>
    public struct DamageModifier(float healthDamageMultiplier, float balanceDamageMultiplier, float flatHealthDamageModifier, float flatBalanceDamageModifier)
    {
        public float HealthDamageMultiplier = healthDamageMultiplier;
        public float BalanceDamageMultiplier = balanceDamageMultiplier;
        public float FlatHealthDamageModifier = flatHealthDamageModifier;
        public float FlatBalanceDamageModifier = flatBalanceDamageModifier;

        public static DamageModifier Default => new DamageModifier(1, 1, 0, 0);
        
        public DamageModifier(DamageModifier source) : this(source.HealthDamageMultiplier, source.BalanceDamageMultiplier, source.FlatHealthDamageModifier, source.FlatBalanceDamageModifier) { }
    }
}

