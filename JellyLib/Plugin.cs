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
using JellyLib.Steamworks;
using JellyLib.WeaponUtils;
using Lua;
using UnityEngine;

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
            script.Globals["DamageSystemExtension"] = typeof(DamageSystemProxy);
            script.Globals["DamageModifier"] = typeof(DamageModifierProxy);
            script.Globals["DamageCalculationPhase"] = typeof(DamageCalculationPhase);
            script.Globals["WeaponUtils"] = typeof(WeaponUtilsProxy);
            script.Globals["WeaponOverride"] = typeof(WeaponOverrideProxy);
            script.Globals["SteamworksExtension"] = typeof(SteamworksProxy);
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
            UserData.RegisterType(typeof(WeaponUtilsProxy), InteropAccessMode.Default, null);
            UserData.RegisterType(typeof(WeaponOverrideProxy), InteropAccessMode.Default, null);
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion((Script s, WeaponOverride v) => DynValue.FromObject(s, WeaponOverrideProxy.New(v)));
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(WeaponOverride), (DynValue v) => v.ToObject<WeaponOverrideProxy>()._value);
            UserData.RegisterType(typeof(SteamworksProxy), InteropAccessMode.Default, null);
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
            __result = allowedTypesList.ToArray();
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

        RavenfieldExtensions.AVAILABLE_EXTENSIONS_LOWERCASE = RavenfieldExtensions.AVAILABLE_EXTENSIONS_LOWERCASE.AddToArray("jellylib");
    }

    private void OnGUI()
    {
        if (!GameManager.IsInMainMenu())
            return;
        if (!WeaponUtils.WeaponUtils.DoneLoading)
            return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 260f, 10f, 250f, 200f), string.Empty);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Dump Weapon Names"))
            WeaponUtils.WeaponUtils.DumpWeaponNames();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}
