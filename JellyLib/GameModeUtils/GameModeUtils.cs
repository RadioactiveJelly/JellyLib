using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Runtime.CompilerServices;
using Ravenfield.SpecOps;
using System.Collections.Generic;

namespace JellyLib.GameModeUtils
{
    public static class GameModeUtils
    {
        public static Vector3 GetSpecOpsAttackerSpawn()
        {
            if (!GameModeBase.activeGameMode)
                return Vector3.zero;

            var specOpsMode = GameModeBase.activeGameMode as SpecOpsMode;
            if (specOpsMode == null)
                return Vector3.zero;

            return specOpsMode.attackerSpawnPosition;
        }

        public static void SpecOpsSpawnSequence()
        {
            if (!GameModeBase.activeGameMode)
                return;
            
            var specOpsMode = GameModeBase.activeGameMode as SpecOpsMode;
            if (specOpsMode == null)
                return;
            
            MethodInfo methodInfo = typeof(SpecOpsMode).GetMethod("SpawnAttackers", BindingFlags.NonPublic | BindingFlags.Instance);
            if(methodInfo != null)
                methodInfo.Invoke(specOpsMode, null);
            
            FieldInfo fieldInfo = typeof(SpecOpsMode).GetField("gameIsRunning", BindingFlags.NonPublic | BindingFlags.Instance);
            if(fieldInfo != null)
                fieldInfo.SetValue(specOpsMode, true);
            
            specOpsMode.dialog.OnPlayerAssumesControl();
            FieldInfo introActionField = typeof(SpecOpsMode).GetField("introAction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (introActionField != null)
            {
                var timedAction = introActionField.GetValue(specOpsMode);
                MethodInfo startInfo = timedAction.GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
                if(startInfo != null)
                    startInfo.Invoke(timedAction, null);
            }
        }

        public static IList<SpecOpsObjective> GetSpecOpsObjectives()
        {
            if (!GameModeBase.activeGameMode)
                return null;
            
            var specOpsMode = GameModeBase.activeGameMode as SpecOpsMode;
            if (specOpsMode == null)
                return null;

            return specOpsMode.activeObjectives;
        }

        public static bool DefaultSpecOpsSpawnSequence { get; set; } = true;
    }

    [HarmonyPatch(typeof(SpecOpsMode), nameof(SpecOpsMode.PlayerAcceptedLoadoutFirstTime))]
    public class PatchSpecOpsSequence
    {
        static bool Prefix(SpecOpsMode __instance)
        {
            return GameModeUtils.DefaultSpecOpsSpawnSequence;
        }
    }
}

