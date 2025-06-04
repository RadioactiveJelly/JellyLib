using Lua;
using MoonSharp.Interpreter;
using Lua.Proxy;
using JellyLib.DamageSystem;

namespace JellyLib.Extensions
{
    /// <summary>
    /// Extensions for the Actor class. Allows some methods to be directly called via an ActorProxy.
    /// </summary>
    public static class ActorExtensions
    {
        public static ScriptEventProxy onBeforeActorDamageCalculation(this ActorProxy actorProxy)
        {
            var actor = actorProxy._value;
            if (actor == null)
                return null;
            
            var actorDamageData = DamageSystem.DamageSystem.Instance.GetActorData(actor.actorIndex);
            return actorDamageData == null ? null : ScriptEventProxy.New(actorDamageData.onBeforeActorDamageCalculation);
        }

        public static ScriptEventProxy onBeforeActorLateDamageCalculation(this ActorProxy actorProxy)
        {
            var actor = actorProxy._value;
            if (actor == null)
                return null;
            
            var actorDamageData = DamageSystem.DamageSystem.Instance.GetActorData(actor.actorIndex);
            return actorDamageData == null ? null : ScriptEventProxy.New(actorDamageData.onBeforeActorLateDamageCalculation);
        }
        
        public static ScriptEventProxy onAfterActorDamageCalculation(this ActorProxy actorProxy)
        {
            var actor = actorProxy._value;
            if (actor == null)
                return null;
            
            var actorDamageData = DamageSystem.DamageSystem.Instance.GetActorData(actor.actorIndex);
            return actorDamageData == null ? null : ScriptEventProxy.New(actorDamageData.onAfterActorDamageCalculation);
        }
        
        public static ScriptEventProxy onAfterDamageApplied(this ActorProxy actorProxy)
        {
            var actor = actorProxy._value;
            if (actor == null)
                return null;
            
            var actorDamageData = DamageSystem.DamageSystem.Instance.GetActorData(actor.actorIndex);
            return actorDamageData == null ? null : ScriptEventProxy.New(actorDamageData.onAfterDamageApplied);
        }
    }
}

