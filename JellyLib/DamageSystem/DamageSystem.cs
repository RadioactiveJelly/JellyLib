using System.Collections.Generic;
using HarmonyLib;
using Lua;
using MoonSharp.Interpreter;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using JellyLib.EventExtensions;
using Lua.Wrapper;
using Ravenfield.Trigger;
using Debug = UnityEngine.Debug;

namespace JellyLib.DamageSystem
{
    public enum DamageRelationship
    {
        Unknown = -1,
        PlayerToSelf = 0,
        PlayerToAlly = 1,
        PlayerToEnemy = 2,
        AllyToPlayer = 3,
        AllyToAlly = 4,
        AllyToEnemy = 5,
        EnemyToPlayer = 6,
        EnemyToAlly = 7,
        EnemyToEnemy = 8
    }
    
    public class DamageSystem
    {
        private readonly Dictionary<int, ActorDamageData> _actorData = new();
        
        private static DamageSystem _instance;
        public static DamageSystem Instance => _instance ??= new DamageSystem();

        private readonly Dictionary<DamageRelationship, float> _healthDamageGlobalMultipliers = new ()
        {
            [DamageRelationship.Unknown] = 1,
            [DamageRelationship.PlayerToSelf] = 1,
            [DamageRelationship.PlayerToAlly] = 1,
            [DamageRelationship.PlayerToEnemy] = 1,
            [DamageRelationship.AllyToPlayer] = 1,
            [DamageRelationship.AllyToAlly] = 1,
            [DamageRelationship.AllyToEnemy] = 1,
            [DamageRelationship.EnemyToPlayer] = 1,
            [DamageRelationship.EnemyToAlly] = 1,
            [DamageRelationship.EnemyToEnemy] = 1,
        };
        
        private readonly Dictionary<DamageRelationship, float> _balanceDamageMultipliers = new ()
        {
            [DamageRelationship.Unknown] = 1,
            [DamageRelationship.PlayerToSelf] = 1,
            [DamageRelationship.PlayerToAlly] = 1,
            [DamageRelationship.PlayerToEnemy] = 1,
            [DamageRelationship.AllyToPlayer] = 1,
            [DamageRelationship.AllyToAlly] = 1,
            [DamageRelationship.AllyToEnemy] = 1,
            [DamageRelationship.EnemyToPlayer] = 1,
            [DamageRelationship.EnemyToAlly] = 1,
            [DamageRelationship.EnemyToEnemy] = 1,
        };

        public void SetGlobalHealthDamageMultiplier(DamageRelationship relationship, float value)
        {
            _healthDamageGlobalMultipliers[relationship] = value;
        }

        public float GetGlobalHealthDamageMultiplier(DamageRelationship relationship)
        {
            _healthDamageGlobalMultipliers.TryGetValue(relationship, out var value);
            return value;
        }
        
        public void SetGlobalBalanceDamageMultiplier(DamageRelationship relationship, float value)
        {
            _balanceDamageMultipliers[relationship] = value;
        }
        
        public float GetGlobalBalanceDamageMultiplier(DamageRelationship relationship)
        {
            _healthDamageGlobalMultipliers.TryGetValue(relationship, out var value);
            return value;
        }
        
        public ActorDamageData GetActorData(int actorId)
        {
            if (_actorData.TryGetValue(actorId, out var data))
            {
                return data;
            }
            var actorDamageData = new ActorDamageData();
            _actorData.Add(actorId, actorDamageData);
            return actorDamageData;
        }

        public ActorDamageData RegisterActor(Actor actor)
        {
            if (_actorData.TryGetValue(actor.actorIndex, out var data))
                return null;
            var actorDamageData = new ActorDamageData();
            _actorData.Add(actor.actorIndex, actorDamageData);
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

        /// <summary>
        /// Calculates the damage the actor will take modified by the global damage multipliers.
        /// Returns false if both balance damage and health damage were set to 0.
        /// </summary>
        /// <param name="targetActor"></param>
        /// <param name="damageInfo"></param>
        /// <returns></returns>
        public bool CalculateGlobalDamage(Actor targetActor, ref DamageInfo damageInfo)
        {
            var relationship = DetermineDamageRelationship(damageInfo.sourceActor, targetActor);
            //Plugin.Logger.LogInfo($"Actor relationship is: {relationship.ToString()}");
            
            var globalHealthDamageMultiplier = GetGlobalHealthDamageMultiplier(relationship);
            var globalBalanceDamageMultiplier = GetGlobalBalanceDamageMultiplier(relationship);
            
            damageInfo.healthDamage *= globalHealthDamageMultiplier;
            damageInfo.balanceDamage *= globalBalanceDamageMultiplier;

            if (damageInfo.healthDamage == 0 || damageInfo.balanceDamage == 0)
                return false;
            return true;
        }
        
        /// <summary>
        /// Calculates the damage the target actor will take. Takes into account any existing modifiers on the source actor and the target actor.
        /// </summary>
        /// <param name="targetActor">The target actor.</param>
        /// <param name="damageInfo">The damage info.</param>
        public void CalculateDamage(Actor targetActor, ref DamageInfo damageInfo)
        {
            var stopwatch = Stopwatch.StartNew();
            
            //Early Phase
            var earlyResult = CalculateDamagePhase(targetActor, damageInfo, DamageCalculationPhase.Early);
            damageInfo.healthDamage = earlyResult.Item1;
            damageInfo.balanceDamage = earlyResult.Item2;
            
            if(_actorData.TryGetValue(targetActor.actorIndex, out var data))
                data.onBeforeActorLateDamageCalculation?.Invoke(targetActor,damageInfo);
            
            if(damageInfo.sourceActor && !damageInfo.sourceActor.aiControlled)
                EventsManager.events.onPlayerDealtDamageLateDamageCalculation?.Invoke(damageInfo, new HitInfo(targetActor));
            
            //Late Phase
            var finalResult = CalculateDamagePhase(targetActor, damageInfo, DamageCalculationPhase.Late);
            damageInfo.healthDamage = finalResult.Item1;
            damageInfo.balanceDamage = finalResult.Item2;
            
            damageInfo.healthDamage = Mathf.Clamp(damageInfo.healthDamage, 0, float.MaxValue);
            damageInfo.balanceDamage = Mathf.Clamp(damageInfo.balanceDamage, 0, float.MaxValue);
            
            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 1000) 
                Plugin.Logger.LogWarning($"[DamageSystem.CalculateDamage] Operation took {stopwatch.ElapsedMilliseconds}ms to calculate damage. This is longer than usual.");
        }

        private (float, float) CalculateDamagePhase(Actor targetActor, DamageInfo damageInfo, DamageCalculationPhase phase)
        {
            var phaseHealthDamage = damageInfo.healthDamage;
            var phaseBalanceDamage = damageInfo.balanceDamage;
            
            if(damageInfo.sourceActor && _actorData.TryGetValue(damageInfo.sourceActor.actorIndex, out var sourceActorData))
            {
                if (sourceActorData.OutgoingDamageData.TryGetValue(damageInfo.type, out var damageData))
                {
                    var finalModifiers = CalculateModifiersAdditive(damageData.DamageModifiers.Values.Where(m => m.DamageCalculationPhase == phase));

                    phaseHealthDamage *= finalModifiers.Item1;
                    phaseBalanceDamage *= finalModifiers.Item2;
                    phaseHealthDamage += finalModifiers.Item3;
                    phaseBalanceDamage += finalModifiers.Item4;
                }
            }
            if (_actorData.TryGetValue(targetActor.actorIndex, out var targetActorData))
            {
                if (targetActorData.IncomingDamageData.TryGetValue(damageInfo.type, out var damageData))
                {
                    var finalModifiers = CalculateModifiersMultiplicative(damageData.DamageModifiers.Values.Where(m => m.DamageCalculationPhase == phase));
                    
                    phaseHealthDamage *= finalModifiers.Item1;
                    phaseBalanceDamage *= finalModifiers.Item2;
                    phaseHealthDamage -= finalModifiers.Item3;
                    phaseBalanceDamage -= finalModifiers.Item4;
                }
            }
            
            return (phaseHealthDamage, phaseBalanceDamage);
        }
        
        private (float, float, float, float) CalculateModifiersMultiplicative(IEnumerable<DamageModifier> modifiers)
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
        
        private (float, float, float, float) CalculateModifiersAdditive(IEnumerable<DamageModifier> modifiers)
        {
            //Health multiplier, balance multiplier, flat health modifier, flat balance modifier
            var finalModifierValue = (1f, 1f, 0f, 0f);
            foreach (var modifier in modifiers)
            {
                finalModifierValue.Item1 += modifier.HealthDamageMultiplier;
                finalModifierValue.Item2 += modifier.BalanceDamageMultiplier;
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
            if (_actorData.TryGetValue(actor.actorIndex, out var data))
            {
                data.AddIncomingDamageModifier(damageType, modifierName, damageModifier);
            }
            else
            {
                var newData = new ActorDamageData();
                newData.AddIncomingDamageModifier(damageType, modifierName, damageModifier);
                _actorData.Add(actor.actorIndex, newData);
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
            if (_actorData.TryGetValue(actor.actorIndex, out var data))
            {
                data.AddOutgoingDamageModifier(damageType, modifierName, damageModifier);
            }
            else
            {
                var newData = new ActorDamageData();
                newData.AddOutgoingDamageModifier(damageType, modifierName, damageModifier);
                _actorData.Add(actor.actorIndex, newData);
            }
        }

        public void RemoveIncomingDamageModifier(Actor actor, DamageInfo.DamageSourceType damageType, string modifierName)
        {
            if (_actorData.TryGetValue(actor.actorIndex, out var data))
            {
                data.RemoveIncomingDamageModifier(damageType, modifierName);
            }
        }

        public void RemoveOutgoingDamageModifier(Actor actor, DamageInfo.DamageSourceType damageType, string modifierName)
        {
            if (_actorData.TryGetValue(actor.actorIndex, out var data))
            {
                data.RemoveOutgoingDamageModifier(damageType, modifierName);
            }
        }

        public void Clear()
        {
            foreach (var actorDamageData in _actorData.Values)
            {
                actorDamageData.onBeforeActorDamageCalculation.RemoveInvalidListeners();
                actorDamageData.onAfterActorDamageCalculation.RemoveInvalidListeners();
            }
            _actorData.Clear();
        }

        private DamageRelationship DetermineDamageRelationship(Actor sourceActor, Actor targetActor)
        {
            if (sourceActor == null)
                return DamageRelationship.Unknown;
            if (targetActor == null)
                return DamageRelationship.Unknown;
            
            var playerTeam = GameManager.PlayerTeam();
            var sourceActorTeam = sourceActor.team;
            var targetActorTeam = targetActor.team;
            var isSourcePlayer = !sourceActor.aiControlled;
            var isPlayerAlly = sourceActorTeam == playerTeam;
            
            if (isSourcePlayer)
            {
                if (sourceActor == targetActor)
                    return DamageRelationship.PlayerToSelf;
                return targetActorTeam == playerTeam ? DamageRelationship.PlayerToAlly : DamageRelationship.PlayerToEnemy;
            }

            if (isPlayerAlly)
            {
                if (!targetActor.aiControlled)
                    return DamageRelationship.AllyToPlayer;
                return sourceActorTeam == targetActorTeam ? DamageRelationship.AllyToAlly : DamageRelationship.AllyToEnemy;
            }

            if (!targetActor.aiControlled)
                return DamageRelationship.EnemyToPlayer;
            return sourceActorTeam == targetActorTeam ? DamageRelationship.EnemyToEnemy : DamageRelationship.EnemyToAlly;
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
                
                modifier.HealthDamageMultiplier = 1 - modifier.HealthDamageMultiplier;
                modifier.BalanceDamageMultiplier = 1 - modifier.BalanceDamageMultiplier;
                
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
            }

            public void RemoveIncomingDamageModifier(DamageInfo.DamageSourceType damageType, string modifierName)
            {
                if (!IncomingDamageData.TryGetValue(damageType, out var damageData))
                    return;
                
                damageData.DamageModifiers.Remove(modifierName);
            }

            public void RemoveOutgoingDamageModifier(DamageInfo.DamageSourceType damageType, string modifierName)
            {
                if (!OutgoingDamageData.TryGetValue(damageType, out var damageData))
                    return;
                
                damageData.DamageModifiers.Remove(modifierName);
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
            //Invoked after all damage calculation is finished but BEFORE damage is applied to the actor.
            public ScriptEvent<Actor, DamageInfo> onAfterActorDamageCalculation { get; protected set; } = new();

            
            [CallbackSignature(new string[]
            {
                "targetActor",
                "damageInfo"
            })]
            //Invoked after damage is calculated and applied to the actor.
            public ScriptEvent<Actor, DamageInfo> onAfterDamageApplied { get; protected set; } = new();
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
        static bool Prefix(Actor __instance, ref DamageInfo info, ref bool __result)
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
            
            //Abort damage calculation if damage got zeroed out by global modifiers.
            //Requires both balance and health damage to be 0.
            var willDamage = DamageSystem.Instance.CalculateGlobalDamage(__instance, ref info);
            if (!willDamage)
            {
                __result = false;
                return false;
            }
            
            var actorData = DamageSystem.Instance.GetActorData(__instance.actorIndex);
            actorData.onBeforeActorDamageCalculation?.Invoke(__instance, info);
            if(info.sourceActor && !info.sourceActor.aiControlled)
                EventsManager.events.onPlayerDealtDamageBeforeDamageCalculation?.Invoke(info, new HitInfo(__instance));
            
            DamageSystem.Instance.CalculateDamage(__instance, ref info);
            
            actorData.onAfterActorDamageCalculation?.Invoke(__instance, info);
            return true;
        }

        static void Postfix(Actor __instance, ref DamageInfo info, bool __result)
        {
            //If damage was aborted for any reason.
            if (!__result)
                return;
            
            var actorData = DamageSystem.Instance.GetActorData(__instance.actorIndex);
            actorData.onAfterDamageApplied?.Invoke(__instance, info);
            if(info.sourceActor &&!info.sourceActor.aiControlled)
                EventsManager.events.onPlayerDealtDamageAfterDamageCalculation?.Invoke(info, new HitInfo(__instance));
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.ReturnToMenu))]
    public class PatchReturnToMenu
    {
        static bool Prefix(GameManager __instance)
        {
            DamageSystem.Instance.Clear();
            Plugin.Logger.LogInfo($"{nameof(DamageSystem)}.{nameof(GameManager.ReturnToMenu)}.Prefix: Cleared damage data.");
            return true;
        }
    }
    
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.RestartLevel))]
    public class PatchRestartLevel
    {
        static bool Prefix(GameManager __instance)
        {
            DamageSystem.Instance.Clear();
            Plugin.Logger.LogInfo($"{nameof(DamageSystem)}.{nameof(GameManager.RestartLevel)}.Prefix: Cleared damage data.");
            return true;
        }
    }
    
    [HarmonyPatch(typeof(ActorManager), nameof(ActorManager.Register))]
    public class PatchRegisterActor
    {
        static bool Prefix(Actor actor)
        {
            Plugin.Logger.LogInfo($"[{nameof(DamageSystem)}.{nameof(ActorManager.Register)}.Prefix].Registered: {actor.name}");
            var actorDamageData = new DamageSystem.ActorDamageData();
            DamageSystem.Instance.RegisterActor(actor);
            return true;
        }
    }
}

