using System;
using Lua;
using Lua.Proxy;
using MoonSharp.Interpreter;

namespace JellyLib.DamageSystem
{
    [Proxy(typeof(DamageSystem))]
    public class DamageSystemProxy : IProxy
    {
        public static void AddIncomingDamageModifier(ActorProxy actor, DamageInfo.DamageSourceType damageType,DamageCalculationPhase damageCalculationPhase, string modifierName,
            float healthDamageMultiplier, float balanceDamageMultiplier, float flatHealthDamageMultiplier, float flatBalanceDamageMultiplier)
        {
            if (!actor._value)
            {
                throw new ScriptRuntimeException("actor cannot be null");
            }
            DamageSystem.Instance.AddIncomingDamageModifier(actor._value, damageType,modifierName, damageCalculationPhase, healthDamageMultiplier, balanceDamageMultiplier, flatHealthDamageMultiplier, flatBalanceDamageMultiplier);
        }

        public static void AddIncomingDamageModifier(ActorProxy actor, DamageInfo.DamageSourceType damageType, string modifierName,
            DamageModifierProxy damageModifier)
        {
            if (!actor._value)
            {
                throw new ScriptRuntimeException("actor cannot be null");
            }
            if (damageModifier == null)
            {
                throw new ScriptRuntimeException("damageModifier cannot be null");
            }
            DamageSystem.Instance.AddIncomingDamageModifier(actor._value, damageType, modifierName, damageModifier._value);
        }
        
        public static void AddOutgoingDamageModifier(ActorProxy actor, DamageInfo.DamageSourceType damageType, string modifierName,
            DamageCalculationPhase damageCalculationPhase,float healthDamageMultiplier, float balanceDamageMultiplier, float flatHealthDamageMultiplier, float flatBalanceDamageMultiplier)
        {
            if (!actor._value)
            {
                throw new ScriptRuntimeException("actor cannot be null");
            }
            DamageSystem.Instance.AddOutgoingDamageModifier(actor._value, damageType, modifierName,damageCalculationPhase, healthDamageMultiplier, balanceDamageMultiplier, flatHealthDamageMultiplier, flatBalanceDamageMultiplier);
        }
        
        public static void AddOutgoingDamageModifier(ActorProxy actor, DamageInfo.DamageSourceType damageType, string modifierName,
            DamageModifierProxy damageModifier)
        {
            if (!actor._value)
            {
                throw new ScriptRuntimeException("actor cannot be null");
            }
            if (damageModifier == null)
            {
                throw new ScriptRuntimeException("damageModifier cannot be null");
            }
            DamageSystem.Instance.AddOutgoingDamageModifier(actor._value, damageType, modifierName, damageModifier._value);
        }

        public static void RemoveIncomingDamageModifier(ActorProxy actor, DamageInfo.DamageSourceType damageType,
            string modifierName)
        {
            if (!actor._value)
            {
                throw new ScriptRuntimeException("actor cannot be null");
            }
            DamageSystem.Instance.RemoveIncomingDamageModifier(actor._value, damageType, modifierName);
        }

        public static void RemoveOutgoingDamageModifier(ActorProxy actor, DamageInfo.DamageSourceType damageType,
            string modifierName)
        {
            if (!actor._value)
            {
                throw new ScriptRuntimeException("actor cannot be null");
            }
            DamageSystem.Instance.RemoveOutgoingDamageModifier(actor._value, damageType, modifierName);
        }

        public static void AddOnBeforeDamageCalculationEvent(DynValue script, ActorProxy actor, string methodName)
        {
            if (!actor._value)
            {
                throw new ScriptRuntimeException("actor cannot be null");
            }
            DamageSystem.Instance.AddOnBeforeDamageCalculationEvent(script,actor._value, methodName);
        }

        public static void RemoveOnBeforeDamageCalculationEvent(DynValue script, ActorProxy actor, string methodName)
        {
            if (!actor._value)
            {
                throw new ScriptRuntimeException("actor cannot be null");
            }
            DamageSystem.Instance.RemoveOnBeforeDamageCalculationEvent(script,actor._value, methodName);
        }
        
        public static void AddOnAfterDamageCalculationEvent(DynValue script, ActorProxy actor, string methodName)
        {
            if (!actor._value)
            {
                throw new ScriptRuntimeException("actor cannot be null");
            }
            DamageSystem.Instance.AddOnAfterDamageCalculationEvent(script,actor._value, methodName);
        }

        public static void RemoveOnAfterDamageCalculationEvent(DynValue script, ActorProxy actor, string methodName)
        {
            if (!actor._value)
            {
                throw new ScriptRuntimeException("actor cannot be null");
            }
            DamageSystem.Instance.RemoveOnAfterDamageCalculationEvent(script,actor._value, methodName);
        }
        
        public static void AddOnBeforeLateDamageCalculationEvent(DynValue script, ActorProxy actor, string methodName)
        {
            if (!actor._value)
            {
                throw new ScriptRuntimeException("actor cannot be null");
            }
            DamageSystem.Instance.AddOnBeforeLateDamageCalculationEvent(script,actor._value, methodName);
        }

        public static void RemoveOnBeforeLateDamageCalculationEvent(DynValue script, ActorProxy actor, string methodName)
        {
            if (!actor._value)
            {
                throw new ScriptRuntimeException("actor cannot be null");
            }
            DamageSystem.Instance.RemoveOnBeforeLateDamageCalculationEvent(script,actor._value, methodName);
        }
        
        [MoonSharpHidden]
        public object GetValue()
        {
            throw new InvalidOperationException($"{nameof(DamageSystem)} is static.");
        }
    }
}

