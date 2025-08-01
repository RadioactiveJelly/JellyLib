﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Lua;
using UnityEngine;
using HarmonyLib;
using JellyLib.DamageSystem;
using Lua.Proxy;

namespace JellyLib.EventExtensions
{
    [GlobalInstance]
    [Include]
    [Name("ExtendedGameEvents")]
    public class RavenscriptEventExtensions : ScriptEventManager
    {
        [CallbackSignature(new string[]
        {
            "projectile",
            "raycastHit"
        })]
        public ScriptEvent<Projectile, RaycastHit> onProjectileLandOnTerrain { get; protected set; }
        
        [CallbackSignature(new string[]
        {
            "sourceActor",
            "targetActor",
            "success"
        })]
        //Invoked when an actor is healed by a medipack.
        public ScriptEvent<Actor, Actor, bool> onMedipackResupply { get;protected set; }
        
        [CallbackSignature(new string[]
        {
            "sourceActor",
            "targetActor",
            "success"
        })]
        //Invoked when an actor is healed by a ammo box.
        public ScriptEvent<Actor, Actor, bool> onAmmoBoxResupply { get; protected set; }
        
        [CallbackSignature(new string[]
        {
            "weapon"
        })]
        //Invoked when the player fires any weapon.
        public ScriptEvent<WeaponProxy> onPlayerFireWeapon { get; protected set; }
        
        
        public ScriptEvent<DamageInfo, HitInfo> onPlayerDealtDamageBeforeDamageCalculation { get; protected set; }
        public ScriptEvent<DamageInfo, HitInfo> onPlayerDealtDamageLateDamageCalculation { get; protected set; }
        public ScriptEvent<DamageInfo, HitInfo> onPlayerDealtDamageAfterDamageCalculation { get; protected set; }
    }
    
    [HarmonyPatch(typeof(Projectile), "Travel")]
    public class PatchProjectileLanding
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = new List<CodeInstruction>(instructions);

            int insertionIndex = -1;
            for (int i = 0; i < codeInstructions.Count - 1; i++) // -1 since we will be checking i + 1
            {
                var codeInstruction = codeInstructions[i];
                if (codeInstruction.opcode == OpCodes.Callvirt && ((MethodInfo)codeInstruction.operand).Name == "SpawnDecal")
                {
                    insertionIndex = i;
                    break;
                }
            }

            var newInstructions = new List<CodeInstruction>();
            newInstructions.Add(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(EventsManager), nameof(EventsManager.events))));
            newInstructions.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(RavenscriptEventExtensions), nameof(EventsManager.events.onProjectileLandOnTerrain))));
            newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
            newInstructions.Add(new CodeInstruction(OpCodes.Ldloc_1));
            newInstructions.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ScriptEvent<Projectile, RaycastHit>), nameof(ScriptEvent<Projectile, RaycastHit>.Invoke),new Type[] { typeof(Projectile), typeof(RaycastHit) })));

            if (insertionIndex != -1)
            {
                codeInstructions.InsertRange(insertionIndex, newInstructions);
            }

            return codeInstructions;
        }
    }
    
    [HarmonyPatch(typeof(Medipack), "Resupply")]
    public class PatchMedipackResupply
    {
        /*static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = new List<CodeInstruction>(instructions);

            int insertionIndex = -1;
            for (int i = 0; i < codeInstructions.Count - 1; i++) // -1 since we will be checking i + 1
            {
                var codeInstruction = codeInstructions[i];
                if (codeInstruction.opcode == OpCodes.Stfld && ((FieldInfo)codeInstruction.operand).Name == "expireTime")
                {
                    insertionIndex = i;
                    break;
                }
            }

            var newInstructions = new List<CodeInstruction>();
            newInstructions.Add(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(EventsManager), nameof(EventsManager.events))));
            newInstructions.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(RavenscriptEventExtensions), nameof(EventsManager.events.onMedipackResupply))));
            newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
            newInstructions.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Projectile), nameof(Projectile.killCredit))));
            newInstructions.Add(new CodeInstruction(OpCodes.Ldloca_S, (byte)0));
            newInstructions.Add(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(List<Actor>.Enumerator), nameof(List<Actor>.Enumerator.Current))));
            newInstructions.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ScriptEvent<Actor, Actor>), nameof(ScriptEvent<Actor, Actor>.Invoke),new Type[] { typeof(Actor), typeof(Actor) })));

            if (insertionIndex != -1)
            {
                codeInstructions.InsertRange(insertionIndex, newInstructions);
            }

            return codeInstructions;
        }*/
        
        static bool Prefix(Medipack __instance)
        {
            FieldInfo fieldInfo = typeof(Medipack).GetField("expireTime", BindingFlags.NonPublic | BindingFlags.Instance);

            var expireTime = 0f;
            if (fieldInfo != null)
                expireTime = Convert.ToSingle(fieldInfo.GetValue(__instance));
            
            foreach (Actor actor in ActorManager.AliveActorsInRange(__instance.transform.position, 6f))
            {
                var success = actor.ResupplyHealth();
                EventsManager.events.onMedipackResupply?.Invoke(__instance.killCredit, actor, success);

                if (!success) continue;

                expireTime -= __instance.reducedLifetimePerResupply;
            }

            if(fieldInfo != null)
                fieldInfo.SetValue(__instance, expireTime);
            
            return false;
            
        }
    }
    
    [HarmonyPatch(typeof(Ammobox), "Resupply")]
    public class PatchAmmoBoxResupply
    {
        static bool Prefix(Ammobox __instance)
        {
            foreach (Actor actor in ActorManager.AliveActorsInRange(__instance.transform.position, 6f))
            {
                var success = actor.ResupplyHealth();
                EventsManager.events.onAmmoBoxResupply?.Invoke(__instance.killCredit, actor, success);
            }

            return false;
        }
    }
    
    [HarmonyPatch(typeof(Weapon), "Shoot")]
    public class PatchWeaponFire
    {
        static bool Prefix(Weapon __instance)
        {
            if (__instance.UserIsAI())
                return true;

            //We return a proxy instead of the Weapon instance to prevent errors from child classes that don't have their own proxies.
            //Note: This also means derived classes with their own proxies are also treated as regular weapons and will not have their class specific variables and methods in RS.
            //TODO: Add methods to convert WeaponProxy to a proxy of a derived class.
            var newProxy = new WeaponProxy(__instance);
                
            EventsManager.events.onPlayerFireWeapon?.Invoke(newProxy);
            return true;
        }
    }
}

