using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Lua.Proxy;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JellyLib.FileManager.Proxy;
using JellyLib.EventExtensions.Proxy;
using JellyLib.EventExtensions;
using Lua;

namespace JellyLib;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    internal static string filePath = Paths.BepInExRootPath + "\\files\\";
    private static Harmony _harmonyInstance;
    
    [HarmonyPatch(typeof(Registrar), nameof(Registrar.ExposeTypes))]
    public class PatchExposeTypes
    {
        static bool Prefix(Script script)
        {
            script.Globals["FileManager"] = typeof(FileManagerProxy);
            script.Globals["ExtendedGameEvents"] = typeof(RavenscriptEventExtensionsProxy);
            return true;
        }
    }

    [HarmonyPatch(typeof(Registrar), nameof(Registrar.RegisterTypes))]
    public class PatchRegisterTypes
    {
        static bool Prefix()
        {
            UserData.RegisterType(typeof(FileManagerProxy), InteropAccessMode.Default, null);
            UserData.RegisterType(typeof(RavenscriptEventExtensionsProxy), InteropAccessMode.Default, null);
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<RavenscriptEventExtensions>((Script s, RavenscriptEventExtensions v) => DynValue.FromObject(s, RavenscriptEventExtensionsProxy.New(v)));
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(RavenscriptEventExtensions), (DynValue v) => v.ToObject<RavenscriptEventExtensionsProxy>()._value);
            return true;
        }
    }

    [HarmonyPatch(typeof(Registrar), nameof(Registrar.GetProxyTypes))]
    public class PatchGetProxyTypes
    {
        static void Postfix(ref Type[] __result)
        {
            List<Type> proxyTypesList = new List<Type>(__result);

            proxyTypesList.Add(typeof(FileManagerProxy));
            proxyTypesList.Add(typeof(RavenscriptEventExtensionsProxy));
            __result = proxyTypesList.ToArray();
        }
    }
    
    [HarmonyPatch(typeof(RavenscriptManager), "Awake")]
    public class PatchRavenscriptManager
    {
        static void Postfix(RavenscriptManager __instance)
        {
            __instance.gameObject.AddComponent<EventsManagerPatch>();
        }
    }
    
    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        
        Directory.CreateDirectory(filePath);

        
        _harmonyInstance = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "JellyLib.Patches");
    }
}
