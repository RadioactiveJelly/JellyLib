using Lua.Proxy;
using MoonSharp.Interpreter;
using System;
using JellyLib.FileManager.Wrapper;
using Lua;
using UnityEngine;
using UnityEngine.Bindings;

namespace JellyLib.FileManager.Proxy;

[Proxy(typeof(WFileManager))]
public class FileManagerProxy : IProxy
{
    public static void WriteAllText(ScriptedBehaviourProxy scriptProxy,string path, string content)
    {
        if (scriptProxy._value == null)
            return;
        
        WFileManager.WriteAllText(scriptProxy._value, path, content);
    }

    public static string FormatJson(string content)
    {
        return WFileManager.FormatJson(content);
    }

    public static string ReadAllText(ScriptedBehaviourProxy scriptProxy, string path)
    {
        return scriptProxy._value == null ? string.Empty : WFileManager.ReadAllText(scriptProxy._value, path);
    }

    public static bool FileExists(ScriptedBehaviourProxy scriptProxy, string path)
    {
        return scriptProxy._value != null && WFileManager.FileExists(scriptProxy._value, path);
    }
    
    public static void CreateDirectory(ScriptedBehaviourProxy scriptProxy, string path)
    {
        if (scriptProxy._value == null)
            return;
        
        WFileManager.CreateDirectory(scriptProxy._value, path);
    }
    
    [MoonSharpHidden]
    public object GetValue()
    {
        throw new InvalidOperationException("Proxied type is static.");
    }
}