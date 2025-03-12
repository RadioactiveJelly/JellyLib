using System.Collections.Generic;
using HarmonyLib;
using Lua;
using MoonSharp.Interpreter;
using UnityEngine;
using System.Linq;

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

        public ActorDamageData GetActorData(int actorId)
        {
            if (actorData.TryGetValue(actorId, out var data))
            {
                return data;
            }
            var actorDamageData = new ActorDamageData();
            actorData.Add(actorId, actorDamageData);
            return actorDamageData;
        }

        public void AddOnBeforeDamageCalculationEvent(DynValue script, Actor actor, string methodName)
        {
            var actorDamageData = GetActorData(actor.actorIndex);
            if (actorDamageData == null)
                return;
            
            actorDamageData.onBeforeActorDamageCalculation.AddListener(script,methodName);
        }

        public void RemoveOnBeforeDamageCalculationEvent(DynValue script, Actor actor, string methodName)
        {
            var actorDamageData = GetActorData(actor.actorIndex);
            actorDamageData.onBeforeActorDamageCalculation.RemoveListener(script,methodName);
        }

        public void AddOnAfterDamageCalculationEvent(DynValue script, Actor actor, string methodName)
        {
            var actorDamageData = GetActorData(actor.actorIndex);
            actorDamageData.onAfterActorDamageCalculation.AddListener(script,methodName);
        }

        public void RemoveOnAfterDamageCalculationEvent(DynValue script, Actor actor, string methodName)
        {
            var actorDamageData = GetActorData(actor.actorIndex);
            actorDamageData.onAfterActorDamageCalculation.RemoveListener(script,methodName);
        }
        
        public void AddOnBeforeLateDamageCalculationEvent(DynValue script, Actor actor, string methodName)
        {
            var actorDamageData = GetActorData(actor.actorIndex);
            actorDamageData.onBeforeActorLateDamageCalculation.AddListener(script,methodName);
        }

        public void RemoveOnBeforeLateDamageCalculationEvent(DynValue script, Actor actor, string methodName)
        {
            var actorDamageData = GetActorData(actor.actorIndex);
            actorDamageData.onBeforeActorLateDamageCalculation.RemoveListener(script,methodName);
        }
        
        public void CalculateDamage(Actor targetActor, ref DamageInfo damageInfo)
        {
            //Early Phase
            var earlyResult = CalculateDamagePhase(targetActor, damageInfo, DamageCalculationPhase.Early);
            damageInfo.healthDamage = earlyResult.Item1;
            damageInfo.balanceDamage = earlyResult.Item2;
            
            if(actorData.TryGetValue(targetActor.actorIndex, out var data))
                data.onBeforeActorLateDamageCalculation?.Invoke(targetActor,damageInfo);
            
            //Late Phase
            var finalResult = CalculateDamagePhase(targetActor, damageInfo, DamageCalculationPhase.Late);
            damageInfo.healthDamage = finalResult.Item1;
            damageInfo.balanceDamage = finalResult.Item2;
            
            damageInfo.healthDamage = Mathf.Clamp(damageInfo.healthDamage, 0, float.MaxValue);
            damageInfo.balanceDamage = Mathf.Clamp(damageInfo.balanceDamage, 0, float.MaxValue);
        }

        private (float, float) CalculateDamagePhase(Actor targetActor, DamageInfo damageInfo, DamageCalculationPhase phase)
        {
            var phaseHealthDamage = 0f;
            var phaseBalanceDamage = 0f;

            if(damageInfo.sourceActor && actorData.TryGetValue(targetActor.actorIndex, out var sourceActorData))
            {
                if (sourceActorData.OutgoingDamageData.TryGetValue(damageInfo.type, out var damageData))
                {
                    var finalModifiers = CalculateModifiers(damageData.DamageModifiers.Values.Where(m => m.DamageCalculationPhase == phase));
                    
                    phaseHealthDamage = (damageInfo.healthDamage * finalModifiers.Item1) + finalModifiers.Item3;
                    phaseBalanceDamage = (damageInfo.balanceDamage * finalModifiers.Item2) + finalModifiers.Item4;
                }
            }
            if (actorData.TryGetValue(targetActor.actorIndex, out var targetActorData))
            {
                if (targetActorData.IncomingDamageData.TryGetValue(damageInfo.type, out var damageData))
                {
                    var finalModifiers = CalculateModifiers(damageData.DamageModifiers.Values.Where(m => m.DamageCalculationPhase == phase));
                    
                    phaseHealthDamage = (damageInfo.healthDamage * finalModifiers.Item1) - finalModifiers.Item3;
                    phaseBalanceDamage = (damageInfo.balanceDamage * finalModifiers.Item2) - finalModifiers.Item4;
                }
            }

            return (phaseHealthDamage, phaseBalanceDamage);
        }
        
        private (float, float, float, float) CalculateModifiers(IEnumerable<DamageModifier> modifiers)
        {
            //Health multiplier, balance multiplier, flat health modifier, flat balance modifier
            var finalModifierValue = (1f, 1f, 0f, 0f);
            foreach (var modifier in modifiers)
            {
                finalModifierValue.Item1 *= modifier.HealthDamageMultiplier;
                finalModifierValue.Item2 *= modifier.BalanceDamageMultiplier;
                finalModifierValue.Item3 += modifier.FlatHealthDamageModifier;
                finalModifierValue.Item4 += modifier.FlatBalanceDamageModifier;
            }
            return finalModifierValue;
        }
        
        public void AddIncomingDamageModifier(Actor actor, DamageInfo.DamageSourceType damageType, string modifierName,DamageCalculationPhase damageCalculationPhase, float healthDamageMultiplier, float balanceDamageMultiplier,
            float flatHealthDamageModifier, float flatBalanceDamageModifier)
        {
            var damageModifier = new DamageModifier(damageCalculationPhase,healthDamageMultiplier, balanceDamageMultiplier,
                flatHealthDamageModifier, flatBalanceDamageModifier);
            AddIncomingDamageModifier(actor,damageType, modifierName, damageModifier);
        }

        public void AddIncomingDamageModifier(Actor actor, DamageInfo.DamageSourceType damageType, string modifierName,
            DamageModifier damageModifier)
        {
            if (actorData.TryGetValue(actor.actorIndex, out var data))
            {
                data.AddIncomingDamageModifier(damageType, modifierName, damageModifier);
            }
            else
            {
                var newData = new ActorDamageData();
                newData.AddIncomingDamageModifier(damageType, modifierName, damageModifier);
                actorData.Add(actor.actorIndex, newData);
            }
        }

        public void AddOutgoingDamageModifier(Actor actor, DamageInfo.DamageSourceType damageType, string modifierName,DamageCalculationPhase damageCalculationPhase, float healthDamageMultiplier, float balanceDamageMultiplier,
            float flatHealthDamageModifier, float flatBalanceDamageModifier)
        {
            var damageModifier = new DamageModifier(damageCalculationPhase,healthDamageMultiplier, balanceDamageMultiplier,
                flatHealthDamageModifier, flatBalanceDamageModifier);
            AddOutgoingDamageModifier(actor,damageType, modifierName, damageModifier);
        }

        public void AddOutgoingDamageModifier(Actor actor, DamageInfo.DamageSourceType damageType, string modifierName,
            DamageModifier damageModifier)
        {
            if (actorData.TryGetValue(actor.actorIndex, out var data))
            {
                data.AddOutgoingDamageModifier(damageType, modifierName, damageModifier);
            }
            else
            {
                var newData = new ActorDamageData();
                newData.AddOutgoingDamageModifier(damageType, modifierName, damageModifier);
                actorData.Add(actor.actorIndex, newData);
            }
        }

        public void RemoveIncomingDamageModifier(Actor actor, DamageInfo.DamageSourceType damageType, string modifierName)
        {
            if (actorData.TryGetValue(actor.actorIndex, out var data))
            {
                data.RemoveIncomingDamageModifier(damageType, modifierName);
            }
        }

        public void RemoveOutgoingDamageModifier(Actor actor, DamageInfo.DamageSourceType damageType, string modifierName)
        {
            if (actorData.TryGetValue(actor.actorIndex, out var data))
            {
                data.RemoveOutgoingDamageModifier(damageType, modifierName);
            }
        }

        public void Clear()
        {
            foreach (var actorDamageData in actorData.Values)
            {
                actorDamageData.onBeforeActorDamageCalculation.RemoveInvalidListeners();
                actorDamageData.onAfterActorDamageCalculation.RemoveInvalidListeners();
            }
            actorData.Clear();
        }
        
        /// <summary>
        /// A struct that holds a collection of damage modifiers
        /// </summary>
        public class DamageData()
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
            /// The final flat health damage modifier that will be used in damage calculation.
            /// </summary>
            public float TotalFlatHealthDamageModifier { get; private set; } = 0;
            
            /// <summary>
            /// The final flat balance damage modifier that will be used in damage calculation.
            /// </summary>
            public float TotalFlatBalanceDamageModifier { get; private set; } = 0;

            /// <summary>
            /// Multiplies all damage modifiers together. Used for performance purposes.
            /// </summary>
            public void CalculateModifiers()
            {
                CalculatedBalanceDamageMultiplier = 1;
                CalculatedBalanceDamageMultiplier = 1;
                TotalFlatHealthDamageModifier = 0;
                TotalFlatBalanceDamageModifier = 0;
                
                foreach (var damageModifier in DamageModifiers.Values)
                {
                    CalculatedHealthDamageMultiplier *= damageModifier.HealthDamageMultiplier;
                    CalculatedBalanceDamageMultiplier *= damageModifier.BalanceDamageMultiplier;
                    TotalFlatHealthDamageModifier += damageModifier.FlatHealthDamageModifier;
                    TotalFlatBalanceDamageModifier += damageModifier.FlatBalanceDamageModifier;
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
        public class ActorDamageData
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

            public void AddIncomingDamageModifier(DamageInfo.DamageSourceType damageType, string modifierName, DamageModifier modifier)
            {
                if (!IncomingDamageData.TryGetValue(damageType, out var damageData))
                    return;
                
                AddModifier(damageData, modifierName, modifier);
            }
            
            public void AddOutgoingDamageModifier(DamageInfo.DamageSourceType damageType, string modifierName, DamageModifier modifier)
            {
                if (!OutgoingDamageData.TryGetValue(damageType, out var damageData))
                    return;
                
                AddModifier(damageData, modifierName, modifier);
            }

            private void AddModifier(DamageData damageData, string modifierName, DamageModifier modifier)
            {
                damageData.DamageModifiers[modifierName] = modifier;
                damageData.CalculateModifiers();
            }

            public void RemoveIncomingDamageModifier(DamageInfo.DamageSourceType damageType, string modifierName)
            {
                if (!IncomingDamageData.TryGetValue(damageType, out var damageData))
                    return;
                
                damageData.DamageModifiers.Remove(modifierName);
                damageData.CalculateModifiers();
            }

            public void RemoveOutgoingDamageModifier(DamageInfo.DamageSourceType damageType, string modifierName)
            {
                if (!OutgoingDamageData.TryGetValue(damageType, out var damageData))
                    return;
                
                damageData.DamageModifiers.Remove(modifierName);
                damageData.CalculateModifiers();
            }

            [CallbackSignature(new string[]
            {
                "targetActor",
                "damageInfo"
            })]
            //Invoked after before damage calculation phase and late damage calculation phase.
            public ScriptEvent<Actor, DamageInfo> onBeforeActorDamageCalculation { get; protected set; } = new();

            [CallbackSignature(new string[]
            {
                "targetActor",
                "damageInfo"
            })]
            //Invoked after early damage calculation phase but before late damage calculation phase.
            public ScriptEvent<Actor, DamageInfo> onBeforeActorLateDamageCalculation { get; protected set; } = new();
            
            [CallbackSignature(new string[]
            {
                "targetActor",
                "damageInfo"
            })]
            //Invoked after all damage calculation is finished.
            public ScriptEvent<Actor, DamageInfo> onAfterActorDamageCalculation { get; protected set; } = new();
        }
    }

    public enum DamageCalculationPhase
    {
        Early,
        Late
    }
    
    [HarmonyPatch(typeof(Actor), nameof(Actor.Damage))]
    public class PatchActorDamage
    {
        static bool Prefix(Actor __instance, ref DamageInfo info)
        {
            if (__instance.isInvulnerable || __instance.dead)
                return true;
            
            bool isSeated = __instance.IsSeated();
            bool inEnclosedSeat = isSeated && __instance.seat.enclosed;
            bool enclosedDamagedByDirectFire = isSeated && __instance.seat.enclosedDamagedByDirectFire;
            if (!info.isPiercing && inEnclosedSeat && (!enclosedDamagedByDirectFire || info.isSplashDamage))
            {
                return true;
            }
            
            var actorData = DamageSystem.Instance.GetActorData(__instance.actorIndex);
            actorData.onBeforeActorDamageCalculation?.Invoke(__instance, info);
            DamageSystem.Instance.CalculateDamage(__instance, ref info);
            return true;
        }

        static void Postfix(Actor __instance, ref DamageInfo info)
        {
            if (__instance.isInvulnerable || __instance.dead)
                return;
            
            bool isSeated = __instance.IsSeated();
            bool inEnclosedSeat = isSeated && __instance.seat.enclosed;
            bool enclosedDamagedByDirectFire = isSeated && __instance.seat.enclosedDamagedByDirectFire;
            if (!info.isPiercing && inEnclosedSeat && (!enclosedDamagedByDirectFire || info.isSplashDamage))
            {
                return;
            }
            
            var actorData = DamageSystem.Instance.GetActorData(__instance.actorIndex);
            actorData.onAfterActorDamageCalculation?.Invoke(__instance, info);
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

