using BepInEx;
using HarmonyLib;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using System.Reflection;
using System.Collections.Generic;

namespace JellyLib.MoonsharpPatch
{
    [HarmonyPatch(typeof(DispatchingUserDataDescriptor), nameof(DispatchingUserDataDescriptor.Index))]
    public class DispatchingUserDataDescriptorPatch
    {
        static bool Prefix(ref int ___m_ExtMethodsVersion)
        {
            ___m_ExtMethodsVersion = 0;
            return true;
        }

        /*static void Postfix(Script script, object obj, DynValue index, bool isDirectIndexing, ref DynValue __result,
            ref Dictionary<string, IMemberDescriptor> ___m_Members,
            ref int ___m_ExtMethodsVersion,
            ref DispatchingUserDataDescriptor __instance)
        {
            if (!isDirectIndexing)
            {
                IMemberDescriptor mdesc = ___m_Members.GetOrDefault("get_Item").WithAccessOrNull(MemberDescriptorAccess.CanExecute);
                if (mdesc != null)
                {
                    var methodInfo = __instance.GetType().GetMethod("ExecuteIndexer", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (methodInfo != null)
                    {
                        Plugin.Logger.LogError($"Could not find method name with ExecuteIndexer in {__instance.GetType()}");
                        __result = null;
                        return;
                    }
                    
                    object[] args = [mdesc, script, obj, index, null];
                    __result = (DynValue)methodInfo.Invoke(__instance, args);
                    if(__result == null)
                        Plugin.Logger.LogError("Direct indexing failed.");
                    return;
                }
            }
            index = index.ToScalar();
            if (index.Type != DataType.String)
            {
                __result = null;
                Plugin.Logger.LogInfo("Returning null");
                return;
            }
            
            DynValue dynValue = ((__instance.TryIndex(script, obj, index.String) ?? __instance.TryIndex(script, obj, DescriptorHelpers.UpperFirstLetter(index.String))) ?? __instance.TryIndex(script, obj, DescriptorHelpers.Camelify(index.String))) ?? __instance.TryIndex(script, obj, DescriptorHelpers.UpperFirstLetter(DescriptorHelpers.Camelify(index.String)));
            if (dynValue == null && ___m_ExtMethodsVersion < UserData.GetExtensionMethodsChangeVersion())
            {
                Plugin.Logger.LogInfo("Checking extensions");
                ___m_ExtMethodsVersion = UserData.GetExtensionMethodsChangeVersion();
                dynValue = ((__instance.TryIndexOnExtMethod(script, obj, index.String) ?? __instance.TryIndexOnExtMethod(script, obj, DescriptorHelpers.UpperFirstLetter(index.String))) ?? __instance.TryIndexOnExtMethod(script, obj, DescriptorHelpers.Camelify(index.String))) ?? __instance.TryIndexOnExtMethod(script, obj, DescriptorHelpers.UpperFirstLetter(DescriptorHelpers.Camelify(index.String)));
            }
            __result = dynValue;
        }*/
    }

    internal static class Extensions
    {
        public static TValue GetOrDefault<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            TKey key)
        {
            TValue obj;
            return dictionary.TryGetValue(key, out obj) ? obj : default (TValue);
        }

        public static DynValue TryIndex(this DispatchingUserDataDescriptor descriptor, Script script, object obj, string indexName)
        {
            var tryIndexMethod = descriptor.GetType().GetMethod("TryIndex", BindingFlags.NonPublic | BindingFlags.Instance);
            if (tryIndexMethod == null)
                return null;
            
            object[] args = [script, obj, indexName];
            var result = (DynValue)tryIndexMethod.Invoke(descriptor, args);
            if(result == null)
                Plugin.Logger.LogError("TryIndex returned null");
            return result;
        }

        public static DynValue TryIndexOnExtMethod(this DispatchingUserDataDescriptor descriptor, Script script,
            object obj, string indexName)
        {
            var tryIndexMethod = descriptor.GetType().GetMethod("TryIndexOnExtMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            if (tryIndexMethod == null)
                return null;
            
            object[] args = [script, obj, indexName];
            var result = (DynValue)tryIndexMethod.Invoke(descriptor, args);
            if(result == null)
                Plugin.Logger.LogError("TryIndexOnExtMethod returned null");
            return result;
        }
    }
}

