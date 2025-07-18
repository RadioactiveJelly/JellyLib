using Lua;
using MoonSharp.Interpreter;
using Lua.Proxy;
using JellyLib.DamageSystem;
using JellyLib.EventExtensions;

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

        public static void Heal(this ActorProxy actorProxy, float amount, Actor source = null, Weapon weapon = null)
        {
            var actor = actorProxy._value;
            if (actor == null)
                return;

            if (actor.dead)
                return;

            var healInfo = new HealInfo
            {
                targetActor = actor,
                sourceActor = source,
                amountHealed = amount,
                sourceWeapon = weapon
            };

            EventsManager.events.onBeforeActorHealed?.Invoke(healInfo);
            
            actor.health += amount;
            if(actor.health > actor.maxHealth) 
                actor.health = actor.maxHealth;

            EventsManager.events.onAfterActorHealed?.Invoke(healInfo);
        }
    }
}

