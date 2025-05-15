using System;
using System.Collections.Generic;
using Lua;
using Lua.Proxy;
using MoonSharp.Interpreter;
using Ravenfield.SpecOps;
using Ravenfield.Trigger;
using UnityEngine;

namespace JellyLib.GameModeUtils
{
    [Proxy(typeof(GameModeUtils))]
    public class GameModeUtilsProxy : IProxy
    {
        public static Vector3 GetSpecOpsAttackerSpawn()
        {
            return GameModeUtils.GetSpecOpsAttackerSpawn();
        }

        public static bool DefaultSpecOpsSpawnSequence
        {
            get => GameModeUtils.DefaultSpecOpsSpawnSequence;
            set => GameModeUtils.DefaultSpecOpsSpawnSequence = value;
        }

        public static void SpecOpsSpawnSequence()
        {
            GameModeUtils.SpecOpsSpawnSequence();
        }

        public static IList<SpecOpsObjective> GetSpecOpsObjectives()
        {
            return GameModeUtils.GetSpecOpsObjectives();
        }

        public static void EndGame(SpawnPoint.Team winner)
        {
            if (winner == SpawnPoint.Team.Neutral)
            {
                throw new ScriptRuntimeException("Winner cannot be a neutral team!");
            }

            if (!GameModeBase.activeGameMode)
                throw new ScriptRuntimeException("No active game mode detected!");
            
            GameModeBase.activeGameMode.Win((int)winner);
        }
        
        [MoonSharpHidden]
        public object GetValue()
        {
            throw new InvalidOperationException("Proxied type is static.");
        }
    }
}

