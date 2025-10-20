using Lua;
using MoonSharp.Interpreter;
using Lua.Proxy;
using JellyLib.DamageSystem;
using JellyLib.EventExtensions;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using JellyLib.Utilities;
using Lua.Wrapper;
using Ravenfield.Trigger;

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

        public static void Heal(this ActorProxy actorProxy, float amount, Actor source = null, Weapon weapon = null, WeaponManager.WeaponEntry weaponEntry = null)
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
                sourceWeapon = weapon,
                sourceWeaponEntry = weaponEntry
            };

            //If no weapon entry was passed but a weapon was provided, set the weapon entry value automatically.
            if (weaponEntry == null && weapon)
                healInfo.sourceWeaponEntry = weapon.weaponEntry;
            
            actorProxy.Heal(healInfo);
        }

        public static void Heal(this ActorProxy actorProxy, HealInfo healInfo)
        {
            var actor = actorProxy._value;
            if (actor == null)
                return;

            if (actor.dead)
                return;

            EventsManager.events.onBeforeActorHealed?.Invoke(healInfo);
            
            var missingHealth = actor.maxHealth - actor.health;
            var actualAmountHealed = Mathf.Clamp(healInfo.amountHealed, 0, missingHealth);
            
            actor.health += healInfo.amountHealed;
            if(actor.health > actor.maxHealth) 
                actor.health = actor.maxHealth;

            EventsManager.events.onAfterActorHealed?.Invoke(healInfo, actualAmountHealed);
        }

        /// <summary>
        /// Sets an actor as immortal for the next frame of damage calculation.
        /// An immortal actor still takes damage but will not die.
        /// </summary>
        /// <param name="actorProxy"></param>
        public static void SetImmortalThisFrame(this ActorProxy actorProxy)
        {
            var actor = actorProxy._value;
            if(actor == null)
                return;

            var actorData = DamageSystem.DamageSystem.Instance.GetActorData(actor.actorIndex);
            if (actorData == null)
                return;

            actorData.ImmortalThisFrame = true;
        }

        public static SilentSpawnToken SilentSpawnAt(this ActorProxy actorProxy, Vector3 position)
        {
            return actorProxy.SilentSpawnAt(position, Quaternion.identity);
        }
        
        public static SilentSpawnToken SilentSpawnAt(this ActorProxy proxy, Vector3 position, Quaternion rotation, WeaponManager.LoadoutSet forcedLoadout = null)
        {
            var actor = proxy._value;
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            ReflectionUtils.SetPrivateField(actor, "forceAnimatorNeverCull", !actor.aiControlled);
            rotation = Quaternion.Euler(rotation.eulerAngles with
            {
                x = 0.0f,
                z = 0.0f
            });
            ReflectionUtils.SetPrivateField(actor, "wantedAnimatorCullMode", AnimatorCullingMode.CullUpdateTransforms);
            actor.EnableHitboxColliders();
            actor.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            actor.ragdoll.SetDrive(700f, 3f);
            actor.ragdoll.SetControl(true);
            actor.ragdoll.InstantAnimate();
            
            actor.attackersIgnoreEngagementRules = false;
            actor.makesProximityMovementNoise = false;
            actor.visibilityDistanceModifier = 0.0f;
            actor.isScheduledToSpawn = false;
            actor.isInvisibleFollower = false;
            
            actor.SetIdleType(Actor.IdleAnimation.Stand);
            ReflectionUtils.SetPrivateField(actor, "animatedSoldierHeightOffset", 0.0f);
            actor.immersedInWater = false;
            actor.feetAreInWater = false;
            var swimmingAction = ReflectionUtils.GetPrivateField<TimedAction>(actor, "swimmingAction");
            swimmingAction.Stop();
            
            ReflectionUtils.CallPrivateMethod(actor, "CancelClimbObstacle");
            
            var climbObstacleCooldownAction = ReflectionUtils.GetPrivateField<TimedAction>(actor, "climbObstacleCooldownAction");
            climbObstacleCooldownAction.Stop();
            
            var bonusSprintSpeedAction = ReflectionUtils.GetPrivateField<TimedAction>(actor, "bonusSprintSpeedAction");
            bonusSprintSpeedAction.StartLifetime(3f);
            actor.ForceStance(Actor.Stance.Stand);
            if (actor.autoMoveActor)
                actor.SetPositionAndRotation(position, rotation);
            actor.weaponImposterRenderers = new Dictionary<Weapon, Renderer[]>();
            WeaponManager.LoadoutSet loadout = forcedLoadout ?? GameModeBase.activeGameMode.GetLoadout(actor);
            if (!actor.aiControlled)
              RavenscriptManager.events.onActorSelectedLoadout.Invoke(actor, loadout, (AiActorController.LoadoutPickStrategy) null);
            else
              RavenscriptManager.events.onActorSelectedLoadout.Invoke(actor, loadout, ((AiActorController) actor.controller).loadoutStrategy);
            
            ReflectionUtils.CallPrivateMethod(actor, "SpawnLoadoutWeapons", loadout);
            foreach (var weapon in actor.weapons)
            {
                if (weapon == null)
                    continue;
                
                weapon.gameObject.SetActive(false);
            }
            
            if (actor.seat != null)
            { 
                actor.seat.OccupantLeft();
                actor.seat = null;
            }
            actor.CutParachutes();
            var actorIk = ReflectionUtils.GetPrivateField<ActorIk>(actor, "ik");
            actorIk.turnBody = true;
            actorIk.weight = 1f;
            actorIk.SetHandIkEnabled(false, false);
            actor.animator.enabled = true;
            actor.animator.SetLayerWeight(2, 0.0f);
            actor.animator.SetTrigger(Actor.ANIM_PAR_RESET);
            
            actor.fallenOver = false;
            actor.getupAction.Stop();
            actor.needsResupply = false;
            ReflectionUtils.SetPrivateField(actor, "aimAnimationLayerWeight", 1f);
            
            var parachuteDeployAction = ReflectionUtils.GetPrivateField<TimedAction>(actor, "parachuteDeployAction");
            parachuteDeployAction.Stop();
           
            ReflectionUtils.SetPrivateField(actor, "fallStartHeight", position.y);
            var parachuteDeployStunAction = ReflectionUtils.GetPrivateField<TimedAction>(actor, "parachuteDeployStunAction");
            parachuteDeployStunAction.Stop();
            ReflectionUtils.CallPrivateMethod(actor, "ResetParachuteCountdown");
            actor.animator.SetBool(Actor.ANIM_PAR_DEAD, false);
            actor.animator.SetBool(Actor.ANIM_PAR_SEATED, false);
            actor.animator.SetBool(Actor.ANIM_PAR_RELOADING, false);
            actor.animator.SetBool(Actor.ANIM_PAR_CLIMBING, false);
            
            var handle = new SilentSpawnToken
            {
                Actor = actor,
                Position = position,
                Rotation = rotation,
            };
            
            stopwatch.Stop();
            Plugin.Logger.LogInfo(stopwatch.Log("ActorExtensions.SilentSpawn"));
            return handle;
        }
    }

    public class SilentSpawnToken
    {
        public Actor Actor { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public void CompleteSpawn()
        {
            var actor = Actor;
            actor.balance = actor.maxBalance;
            actor.health = actor.maxHealth;
            actor.hasSpawnedAmmoReserve = false;
            actor.hasHeroArmor = false;
            actor.isInvulnerable = false;
            actor.dead = false;
            actor.speedMultiplier = 1f;
            actor.canDeployParachute = true;
            actor.parachuteDeployed = false;
            actor.isScheduledToSpawn = false;
            actor.ladder = null;
            actor.moving = false;
            actor.Show();
            actor.Unfreeze();
            actor.EnableHitboxColliders();
            actor.controller.SpawnAt(Position, Rotation);
            actor.controller.EnableMovement();
            actor.controller.EnableInput();
            ReflectionUtils.CallPrivateMethod(actor, "SwitchToFirstAvailableWeapon");
            
            ActorManager.SetAlive(actor);
            ReflectionUtils.CallPrivateMethod(actor, "UpdateCachedValues");
            RavenscriptManager.events.onActorSpawn.Invoke(actor);
        }
    }

    [Proxy(typeof(DamageModifier))]
    public class SilentSpawnTokenProxy : IProxy
    {
        [MoonSharpHidden]
        public SilentSpawnToken _value;
        
        public object GetValue()
        {
            return _value;
        }
        
        [MoonSharpHidden]
        public SilentSpawnTokenProxy(SilentSpawnToken value)
        {
            _value = value;
        }
        
        public SilentSpawnTokenProxy()
        {
            _value = default;
        }
        
        public Actor actor => _value.Actor;
        public Vector3 position => _value.Position;
        public Quaternion rotation => _value.Rotation;

        public void CompleteSpawn()
        {
            _value.CompleteSpawn();
        }

        public SilentSpawnTokenProxy(SilentSpawnTokenProxy source)
        {
            if (source == null)
            {
                throw new ScriptRuntimeException("argument 'source' is nil");
            }
            _value = source._value;
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static SilentSpawnTokenProxy Call(DynValue _, SilentSpawnTokenProxy source)
        {
            return new SilentSpawnTokenProxy(source);
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static SilentSpawnTokenProxy Call(DynValue _)
        {
            return new SilentSpawnTokenProxy();
        }
        
        [MoonSharpHidden]
        public static SilentSpawnTokenProxy New(SilentSpawnToken source)
        {
            return new SilentSpawnTokenProxy(source);
        }
    }
}

