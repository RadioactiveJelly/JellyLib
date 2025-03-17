using System;
using Lua;
using Lua.Proxy;
using MoonSharp.Interpreter;

namespace JellyLib.DamageSystem
{
    [Proxy(typeof(DamageModifier))]
    public class DamageModifierProxy : IProxy
    {
        [MoonSharpHidden]
        public DamageModifier _value;
        
        public static DamageModifier Default => DamageModifier.Default;

        [MoonSharpHidden]
        public DamageModifierProxy(DamageModifier value)
        {
            _value = value;
        }
        
        public DamageModifierProxy()
        {
            _value = DamageModifier.Default;
        }

        public DamageModifierProxy(DamageModifierProxy source)
        {
            if (source == null)
            {
                throw new ScriptRuntimeException("argument 'source' is nil");
            }
            _value = source._value;
        }

        public DamageModifierProxy(DamageCalculationPhase damageCalculationPhase, float healthDamageMultiplier, float balanceDamageMultiplier,
            float flatHealthDamageMultiplier, float flatBalanceDamageModifier)
        {
            _value = new DamageModifier(damageCalculationPhase, healthDamageMultiplier, balanceDamageMultiplier, flatHealthDamageMultiplier, flatBalanceDamageModifier);
        }

        public float HealthDamageMultiplier
        {
            get => _value.HealthDamageMultiplier;
            set => _value.HealthDamageMultiplier = value;
        }

        public float BalanceDamageMultiplier
        {
            get => _value.BalanceDamageMultiplier;
            set => _value.BalanceDamageMultiplier = value;
        }

        public float FlatHealthDamageModifier
        {
            get => _value.FlatHealthDamageModifier;
            set => _value.FlatHealthDamageModifier = value;
        }

        public float FlatBalanceDamageModifier
        {
            get => _value.FlatBalanceDamageModifier;
            set => _value.FlatBalanceDamageModifier = value;
        }

        public DamageCalculationPhase DamageCalculationPhase
        {
            get => _value.DamageCalculationPhase;
            set => _value.DamageCalculationPhase = value;
        }

        [MoonSharpHidden]
        public static DamageModifierProxy New(DamageModifier modifier)
        {
            return new DamageModifierProxy(modifier);
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static DamageModifierProxy Call(DynValue _,DamageCalculationPhase damageCalculationPhase,float healthDamageMultiplier, float balanceDamageMultiplier,
            float flatHealthDamageMultiplier, float flatBalanceDamageModifier)
        {
            return new DamageModifierProxy(damageCalculationPhase,healthDamageMultiplier, balanceDamageMultiplier, flatHealthDamageMultiplier, flatBalanceDamageModifier);
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static DamageModifierProxy Call(DynValue _, DamageModifierProxy source)
        {
            return new DamageModifierProxy(source);
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static DamageModifierProxy Call(DynValue _)
        {
            return new DamageModifierProxy();
        }
        
        [MoonSharpHidden]
        public object GetValue()
        {
            return _value;
        }
    }
}

