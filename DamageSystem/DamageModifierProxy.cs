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

        public DamageModifierProxy(float healthDamageMultiplier, float balanceDamageMultiplier,
            float flatHealthDamageMultiplier, float flatBalanceDamageModifier)
        {
            _value = new DamageModifier(healthDamageMultiplier, balanceDamageMultiplier, flatHealthDamageMultiplier, flatBalanceDamageModifier);
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

        public static DamageModifierProxy New(DamageModifier modifier)
        {
            return new DamageModifierProxy(modifier);
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static DamageModifierProxy Call(float healthDamageMultiplier, float balanceDamageMultiplier,
            float flatHealthDamageMultiplier, float flatBalanceDamageModifier)
        {
            return new DamageModifierProxy(healthDamageMultiplier, balanceDamageMultiplier, flatHealthDamageMultiplier, flatBalanceDamageModifier);
        }

        // Token: 0x06003901 RID: 14593 RVA: 0x00105E6E File Offset: 0x0010406E
        [MoonSharpUserDataMetamethod("__call")]
        public static DamageModifierProxy Call(DynValue _, DamageModifierProxy source)
        {
            return new DamageModifierProxy(source);
        }

        // Token: 0x06003902 RID: 14594 RVA: 0x00105E76 File Offset: 0x00104076
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

