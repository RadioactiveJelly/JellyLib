using Lua.Proxy;
using MoonSharp.Interpreter;
using System;
using JellyLib.FileManager.Wrapper;

namespace JellyLib.FileManager.Proxy;

[Proxy(typeof(WFileManager))]
public class FileManagerProxy : IProxy
{
    public static void WriteAllText(string path, string content)
    {
        WFileManager.WriteAllText(Plugin.filePath + path, content);
    }

    public static string ReadAllText(string path)
    {
        return WFileManager.ReadAllText(Plugin.filePath + path);
    }

    public static bool FileExists(string path)
    {
        return WFileManager.FileExists(Plugin.filePath + path);
    }
    
    public static void CreateDirectory(string path)
    {
        WFileManager.CreateDirectory(Plugin.filePath + path);
    }
    
    [MoonSharpHidden]
    public object GetValue()
    {
        throw new InvalidOperationException("Proxied type is static.");
    }
}