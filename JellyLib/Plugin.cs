using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Lua.Proxy;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JellyLib.DamageSystem;
using JellyLib.FileManager.Proxy;
using JellyLib.EventExtensions.Proxy;
using JellyLib.EventExtensions;
using JellyLib.GameModeUtils;
using JellyLib.Steamworks;
using JellyLib.WeaponUtils;
using JellyLib.Extensions;
using JellyLib.Utilities;
using Lua;
using Lua.Wrapper;
using Ravenfield.SpecOps;
using UnityEngine;


namespace JellyLib;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    internal static string filePath = Paths.BepInExRootPath + "\\files\\";
    private static Harmony _harmonyInstance;
    private const int EXPECTED_GAME_VERSION = 31;
    
    [HarmonyPatch(typeof(Registrar), nameof(Registrar.ExposeTypes))]
    public class PatchExposeTypes
    {
        static bool Prefix(Script script)
        {
            script.Globals["FileManager"] = typeof(FileManagerProxy);
            script.Globals["ExtendedGameEvents"] = typeof(RavenscriptEventExtensionsProxy);
            script.Globals["DamageSystemExtension"] = typeof(DamageSystemProxy);
            script.Globals["DamageModifier"] = typeof(DamageModifierProxy);
            script.Globals["DamageCalculationPhase"] = typeof(DamageCalculationPhase);
            script.Globals["DamageRelationship"] = typeof(DamageRelationship);
            script.Globals["WeaponUtils"] = typeof(WeaponUtilsProxy);
            script.Globals["WeaponOverride"] = typeof(WeaponOverrideProxy);
            script.Globals["SteamworksExtension"] = typeof(SteamworksProxy);
            script.Globals["GameModeUtils"] = typeof(GameModeUtilsProxy);
            script.Globals["GameObjective"] = typeof(ObjectiveProxy);
            script.Globals["HealInfo"] = typeof(HealInfoProxy);
            script.Globals["JellyLib"] = typeof(JellyLibProxy);
            script.Globals["ObjectPool"] = typeof(ObjectPoolProxy);
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
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion((Script s, RavenscriptEventExtensions v) => DynValue.FromObject(s, RavenscriptEventExtensionsProxy.New(v)));
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(RavenscriptEventExtensions), (DynValue v) => v.ToObject<RavenscriptEventExtensionsProxy>()._value);
            UserData.RegisterType(typeof(DamageSystemProxy), InteropAccessMode.Default, null);
            UserData.RegisterType(typeof(DamageModifierProxy), InteropAccessMode.Default, null);
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion((Script s, DamageModifier v) => DynValue.FromObject(s, DamageModifierProxy.New(v)));
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(DamageModifier), (DynValue v) => v.ToObject<DamageModifierProxy>()._value);
            UserData.RegisterType(typeof(DamageCalculationPhase), InteropAccessMode.Default, null);
            UserData.RegisterType(typeof(DamageRelationship), InteropAccessMode.Default, null);
            UserData.RegisterType(typeof(WeaponUtilsProxy), InteropAccessMode.Default, null);
            UserData.RegisterType(typeof(WeaponOverrideProxy), InteropAccessMode.Default, null);
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion((Script s, WeaponOverride v) => DynValue.FromObject(s, WeaponOverrideProxy.New(v)));
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(WeaponOverride), (DynValue v) => v.ToObject<WeaponOverrideProxy>()._value);
            UserData.RegisterType(typeof(SteamworksProxy), InteropAccessMode.Default, null);
            UserData.RegisterType(typeof(GameModeUtilsProxy), InteropAccessMode.Default, null);
            UserData.RegisterType(typeof(ObjectiveProxy), InteropAccessMode.Default, null);
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion((Script s, SpecOpsObjective v) => DynValue.FromObject(s, ObjectiveProxy.New(v)));
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(SpecOpsObjective), (DynValue v) => v.ToObject<ObjectiveProxy>()._value);
            UserData.RegisterExtensionType(typeof(ActorExtensions));
            UserData.RegisterType(typeof(HealInfoProxy), InteropAccessMode.Default, null);
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion((Script s, HealInfo v) => DynValue.FromObject(s, HealInfoProxy.New(v)));
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(HealInfo), (DynValue v) => v.ToObject<HealInfoProxy>()._value);
            UserData.RegisterType(typeof(JellyLibProxy), InteropAccessMode.Default, null);
            UserData.RegisterType(typeof(ObjectPoolProxy), InteropAccessMode.Default, null);
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion((Script s, ObjectPool v) => DynValue.FromObject(s, ObjectPoolProxy.New(v)));
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(ObjectPool), (DynValue v) => v.ToObject<ObjectPoolProxy>()._value);
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
            proxyTypesList.Add(typeof(DamageSystemProxy));
            proxyTypesList.Add(typeof(DamageModifierProxy));
            proxyTypesList.Add(typeof(WeaponUtilsProxy));
            proxyTypesList.Add(typeof(WeaponOverrideProxy));
            proxyTypesList.Add(typeof(SteamworksProxy));
            proxyTypesList.Add(typeof(GameModeUtilsProxy));
            proxyTypesList.Add(typeof(ObjectiveProxy));
            proxyTypesList.Add(typeof(HealInfoProxy));
            proxyTypesList.Add(typeof(JellyLibProxy));
            proxyTypesList.Add(typeof(ObjectPoolProxy));
            __result = proxyTypesList.ToArray();
        }
    }

    [HarmonyPatch(typeof(Registrar), nameof(Registrar.GetAllowedTypes))]
    public class PatchGetAllowedTypes
    {
        static void Postfix(ref Type[] __result)
        {
            List<Type> allowedTypesList = new List<Type>(__result);
            
            allowedTypesList.Add(typeof(DamageCalculationPhase));
            allowedTypesList.Add(typeof(DamageRelationship));
            __result = allowedTypesList.ToArray();
        }
    }
    
    [HarmonyPatch(typeof(RavenscriptManager), "Awake")]
    public class PatchRavenscriptManager
    {
        static void Postfix(RavenscriptManager __instance)
        {
            __instance.gameObject.AddComponent<EventsManager>();
        }
    }
    
    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        
        Directory.CreateDirectory(filePath);
        
        _harmonyInstance = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "JellyLib.Patches");

        RavenfieldExtensions.AVAILABLE_EXTENSIONS_LOWERCASE = RavenfieldExtensions.AVAILABLE_EXTENSIONS_LOWERCASE.AddToArray("jellylib");
    }

    private void OnGUI()
    {
        if (!GameManager.instance)
            return;
        if (!GameManager.IsInMainMenu())
            return;

        GUILayout.BeginArea(new Rect(Screen.width - 260f, 10f, 250f, 200f), string.Empty);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(EXPECTED_GAME_VERSION != GameManager.instance.buildNumber)
            GUILayout.Box($"<color=yellow>WARNING: JellyLib is not compatible with this version of Ravenfield. Expected {EXPECTED_GAME_VERSION}. Got {GameManager.instance.buildNumber}</color>");
        else
            GUILayout.Box($"JellyLib ({MyPluginInfo.PLUGIN_VERSION})");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        
        if (!WeaponUtils.WeaponUtils.DoneLoading)
            return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 260f, 40f, 250f, 200f), string.Empty);
        //Dump weapon data
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Dump Weapon Names"))
            WeaponUtils.WeaponUtils.DumpWeaponNames();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.EndArea();
    }
}

[Proxy(typeof(Plugin))]
public class JellyLibProxy : IProxy
{
    public static int VersionCompare(string versionString)
    {
        var localVersion = new Version(MyPluginInfo.PLUGIN_VERSION);
        var versionToCompare = new Version(versionString);
        
        return localVersion.CompareTo(versionToCompare);
    }

    public static string GetPluginVersion()
    {
        return MyPluginInfo.PLUGIN_VERSION;
    }
    
    public object GetValue()
    {
        throw new InvalidOperationException("Proxied type is static.");
    }
}
