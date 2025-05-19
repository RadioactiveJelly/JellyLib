using HarmonyLib;
using Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace JellyLib.EventExtensions
{
    [HarmonyPatch(typeof(RavenscriptManager), "ResolveCurrentlyInvokingSourceScript")]
    public class EventsManager : MonoBehaviour
    {
        private RavenscriptEventExtensions _events;

        public static RavenscriptEventExtensions events => _instance._events;

        private static EventsManager _instance;

        private void Awake()
        {
            _instance = this;
            var engine = GameObject.Find("_Managers(Clone)").GetComponent<ScriptEngine>();
            _events = gameObject.AddComponent<RavenscriptEventExtensions>();
            engine.Set(NameAttribute.GetName(typeof(RavenscriptEventExtensions)), this._events);
        }
    }

    [HarmonyPatch(typeof(RavenscriptManager), "ResolveCurrentlyInvokingSourceScript")]
    public class PatchResolveCurrentlyInvokingSourceScript
    {
        static void Postfix(ref ScriptedBehaviour __result)
        {
            if (!EventsManager.events.IsCallStackEmpty())
            {
                ScriptedBehaviour currentInvokingListenerScript =
                    EventsManager.events.GetCurrentEvent().GetCurrentInvokingListenerScript();
                if (currentInvokingListenerScript != null)
                {
                    __result = currentInvokingListenerScript;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(ScriptEvent), "UnsafeInvoke")]
    public class PatchUnsafeInvoke
    {
        static bool Prefix(ScriptEvent __instance)
        {
            if (EventsManager.events.IsCallStackFull())
            {
                StackTraceLogType stackTraceLogType = Application.GetStackTraceLogType(LogType.Error);
                Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
                ScriptConsole.instance.LogError("[Prefix] Event Call Stack is full, escaping event callback. If you see this message, one or more scripts generated an event feedback loop.");
                Application.SetStackTraceLogType(LogType.Error, stackTraceLogType);
                return false;
            }
            EventsManager.events.PushCallStack(__instance);
            return true;
        }
        static void Postfix()
        {
            EventsManager.events.PopCallStack();
        }
    }
    [HarmonyPatch(typeof(ScriptEventCache), "GetOrCreateEvent")]
    public class PatchGetOrCreateEvent
    {
        static bool Prefix(ScriptEventCache __instance,UnityEventBase unityEvent, ref ScriptEventCache.GetOrCreateResult __result, Dictionary<UnityEventBase, ScriptEvent> ___events)
        {
            if (!___events.ContainsKey(unityEvent))
            {
                ScriptEvent scriptEvent = EventsManager.events.CreateEvent();
                ___events.Add(unityEvent, scriptEvent);
                __result = new ScriptEventCache.GetOrCreateResult(scriptEvent, wasCreated: true);
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(ScriptEventCache), "GetOrCreateAction")]
    public class OatchGetOrCreateAction
    {
        static bool Prefix(ScriptEventCache __instance, Action action, ref ScriptEventCache.GetOrCreateResult __result, Dictionary<Action, ScriptEvent> ___actions)
        {
            if (!___actions.ContainsKey(action))
            {
                ScriptEvent scriptEvent = EventsManager.events.CreateEvent();
                ___actions.Add(action, scriptEvent);
                __result = new ScriptEventCache.GetOrCreateResult(scriptEvent, true);
                return false;
            }
            return true;
        }
    }
}

    
    