using System;
using Lua;
using Lua.Proxy;
using MoonSharp.Interpreter;

namespace JellyLib.DamageSystem
{
    [Proxy(typeof(DamageSystem))]
    public class DamageSystemProxy : IProxy
    {
        public static void AddIncomingDamageModifier(Actor actor, DamageInfo.DamageSourceType damageType, string modifierName,
            float healthDamageMultiplier, float balanceDamageMultiplier, float flatHealthDamageMultiplier, float flatBalanceDamageMultiplier)
        {
            DamageSystem.Instance.AddIncomingDamageModifier(actor, damageType, modifierName, healthDamageMultiplier, balanceDamageMultiplier, flatHealthDamageMultiplier, flatBalanceDamageMultiplier);
        }

        public void AddOutgoingDamageModifier(Actor actor, DamageInfo.DamageSourceType damageType, string modifierName,
            float healthDamageMultiplier, float balanceDamageMultiplier, float flatHealthDamageMultiplier, float flatBalanceDamageMultiplier)
        {
            DamageSystem.Instance.AddOutgoingDamageModifier(actor, damageType, modifierName, healthDamageMultiplier, balanceDamageMultiplier, flatHealthDamageMultiplier, flatBalanceDamageMultiplier);
        }
        
        [MoonSharpHidden]
        public object GetValue()
        {
            throw new InvalidOperationException($"{nameof(DamageSystem)} is static.");
        }
    }
}

