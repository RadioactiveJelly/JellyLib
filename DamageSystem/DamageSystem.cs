using System.Collections.Generic;
using HarmonyLib;
using Lua;
using JellyLib.EventExtensions;

namespace JellyLib.DamageSystem
{
    public class DamageSystem
    {
        private static DamageSystem _instance;
        public static DamageSystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new();
                return _instance;
            }
        }
        
        private Dictionary<int, ActorDamageData> actorData = new();
        
        public void CalculateDamage(Actor targetActor, ref DamageInfo damageInfo)
        {
            if (actorData.TryGetValue(targetActor.actorIndex, out var targetActorData))
            {
                if (targetActorData.IncomingDamageData.TryGetValue(damageInfo.type, out var damageData))
                {
                    damageInfo.healthDamage *= damageData.CalculatedHealthDamageMultiplier;
                    damageInfo.balanceDamage *= damageData.CalculatedBalanceDamageMultiplier;
                }
            }
            
            if(damageInfo.sourceActor && actorData.TryGetValue(targetActor.actorIndex, out var sourceActorData))
            {
                if (sourceActorData.OutgoingDamageData.TryGetValue(damageInfo.type, out var damageData))
                {
                    damageInfo.healthDamage *= damageData.CalculatedHealthDamageMultiplier;
                    damageInfo.balanceDamage *= damageData.CalculatedBalanceDamageMultiplier;
                }
            }
        }
        
        public void AddIncomingDamageModifier(Actor actor, DamageInfo.DamageSourceType damageType, string modifierName, float healthDamageMultiplier, float balanceDamageMultiplier,
            float flatHealthDamageModifier, float flatBalanceDamageModifier)
        {
            if (actorData.TryGetValue(actor.actorIndex, out var data))
            {
                data.AddIncomingDamageModifier(damageType, modifierName, healthDamageMultiplier, balanceDamageMultiplier, flatHealthDamageModifier, flatBalanceDamageModifier);
            }
            else
            {
                var newData = new ActorDamageData();
                newData.AddIncomingDamageModifier(damageType, modifierName, healthDamageMultiplier, balanceDamageMultiplier,  flatHealthDamageModifier, flatBalanceDamageModifier);
                actorData.Add(actor.actorIndex, newData);
            }
        }
        
        public void AddOutgoingDamageModifier(Actor actor, DamageInfo.DamageSourceType damageType, string modifierName, float healthDamageMultiplier, float balanceDamageMultiplier,
            float flatHealthDamageModifier, float flatBalanceDamageModifier)
        {
            if (actorData.TryGetValue(actor.actorIndex, out var data))
            {
                data.AddOutgoingDamageModifier(damageType, modifierName, healthDamageMultiplier, balanceDamageMultiplier, flatHealthDamageModifier, flatBalanceDamageModifier);
            }
            else
            {
                var newData = new ActorDamageData();
                newData.AddOutgoingDamageModifier(damageType, modifierName, healthDamageMultiplier, balanceDamageMultiplier, flatHealthDamageModifier, flatBalanceDamageModifier);
                actorData.Add(actor.actorIndex, newData);
            }
        }

        public void Clear()
        {
            actorData.Clear();
        }
        
        

        /// <summary>
        /// A struct that holds a collection of damage modifiers
        /// </summary>
        private class DamageData()
        {
            /// <summary>
            /// A dictionary that holds damage modifiers. Modifiers are multiplied together to calculate a final value.
            /// </summary>
            public Dictionary<string, DamageModifier> DamageModifiers { get; private set; } = new();
            /// <summary>
            /// The final health damage multiplier that will be used in damage calculation.
            /// </summary>
            public float CalculatedHealthDamageMultiplier { get; private set; } = 1;
            /// <summary>
            /// The final balance damage multiplier that will be used in damage calculation.
            /// </summary>
            public float CalculatedBalanceDamageMultiplier { get; private set; } = 1;

            /// <summary>
            /// Multiplies all damage modifiers together. Used for performance purposes.
            /// </summary>
            public void CalculateMultipliers()
            {
                CalculatedBalanceDamageMultiplier = 1;
                CalculatedBalanceDamageMultiplier = 1;
                foreach (var damageModifier in DamageModifiers.Values)
                {
                    CalculatedHealthDamageMultiplier *= damageModifier.HealthDamageMultiplier;
                    CalculatedBalanceDamageMultiplier *= damageModifier.BalanceDamageMultiplier;
                }
            }

            public void ClearDamageModifiers()
            {
                DamageModifiers.Clear();
            }
        }

        /// <summary>
        /// Damage data associated to an actor.
        /// </summary>
        private class ActorDamageData
        {
            public IReadOnlyDictionary<DamageInfo.DamageSourceType, DamageData> IncomingDamageData =
                new Dictionary<DamageInfo.DamageSourceType, DamageData>()
                {
                    { DamageInfo.DamageSourceType.Unknown , new DamageData() },
                    { DamageInfo.DamageSourceType.Projectile , new DamageData() },
                    { DamageInfo.DamageSourceType.Melee , new DamageData() },
                    { DamageInfo.DamageSourceType.Explosion , new DamageData() },
                    { DamageInfo.DamageSourceType.StickyExplosive , new DamageData() },
                    { DamageInfo.DamageSourceType.VehicleRam , new DamageData() },
                    { DamageInfo.DamageSourceType.FallDamage , new DamageData() },
                    { DamageInfo.DamageSourceType.DamageZone , new DamageData() },
                    { DamageInfo.DamageSourceType.Exception , new DamageData() },
                    { DamageInfo.DamageSourceType.Scripted , new DamageData() },
                    { DamageInfo.DamageSourceType.ProjectileShatter , new DamageData() },
                };
            
            public IReadOnlyDictionary<DamageInfo.DamageSourceType, DamageData> OutgoingDamageData =
                new Dictionary<DamageInfo.DamageSourceType, DamageData>()
                {
                    { DamageInfo.DamageSourceType.Unknown , new DamageData() },
                    { DamageInfo.DamageSourceType.Projectile , new DamageData() },
                    { DamageInfo.DamageSourceType.Melee , new DamageData() },
                    { DamageInfo.DamageSourceType.Explosion , new DamageData() },
                    { DamageInfo.DamageSourceType.StickyExplosive , new DamageData() },
                    { DamageInfo.DamageSourceType.VehicleRam , new DamageData() },
                    { DamageInfo.DamageSourceType.FallDamage , new DamageData() },
                    { DamageInfo.DamageSourceType.DamageZone , new DamageData() },
                    { DamageInfo.DamageSourceType.Exception , new DamageData() },
                    { DamageInfo.DamageSourceType.Scripted , new DamageData() },
                    { DamageInfo.DamageSourceType.ProjectileShatter , new DamageData() },
                };

            public void AddIncomingDamageModifier(DamageInfo.DamageSourceType damageType, string modifierName,  float healthDamageMultiplier,
                float balanceDamageMultiplier, float flatHealthDamageModifier, float flatBalanceDamageModifier)
            {
                if (!IncomingDamageData.TryGetValue(damageType, out var damageData))
                    return;
                
                damageData.DamageModifiers[modifierName] = new DamageModifier(healthDamageMultiplier, balanceDamageMultiplier, flatHealthDamageModifier, flatBalanceDamageModifier);
                damageData.CalculateMultipliers();
            }
            
            public void AddOutgoingDamageModifier(DamageInfo.DamageSourceType damageType, string modifierName,  float healthDamageMultiplier,
                float balanceDamageMultiplier,float flatHealthDamageModifier, float flatBalanceDamageModifier)
            {
                if (!OutgoingDamageData.TryGetValue(damageType, out var damageData))
                    return;
                
                damageData.DamageModifiers[modifierName] = new DamageModifier(healthDamageMultiplier, balanceDamageMultiplier, flatHealthDamageModifier,flatBalanceDamageModifier);
                damageData.CalculateMultipliers();
            }
        }
    }
    
    [HarmonyPatch(typeof(Actor), nameof(Actor.Damage))]
    public class PatchActorDamage
    {
        static bool Prefix(Actor __instance, ref DamageInfo info)
        {
            EventsManagerPatch.events.onBeforeActorDamageCalculation?.Invoke(__instance, info);
            DamageSystem.Instance.CalculateDamage(__instance, ref info);
            return true;
        }

        static bool Postfix(Actor __instance, ref DamageInfo info)
        {
            EventsManagerPatch.events.onAfterActorDamageCalculation?.Invoke(__instance, info);
            return true;
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.ReturnToMenu))]
    public class PatchReturnToMenu
    {
        static bool Prefix(GameManager __instance)
        {
            DamageSystem.Instance.Clear();
            return true;
        }
    }
}

