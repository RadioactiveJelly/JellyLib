using System.IO;
using Lua;

namespace JellyLib.FileManager.Wrapper;

public static class WFileManager
{
    public static void WriteAllText(ScriptedBehaviour script, string path, string content)
    {
        var modId = GetModId(script);
        var finalPath = $@"{Plugin.filePath}\{modId}\{path}";
        File.WriteAllText(finalPath, content);
    }

    public static string ReadAllText(ScriptedBehaviour script,string path)
    {
        var modId = GetModId(script);
        var finalPath = $@"{Plugin.filePath}\{modId}\{path}";
        return File.ReadAllText(finalPath);
    }

    public static bool FileExists(ScriptedBehaviour script,string path)
    {
        var modId = GetModId(script);
        var finalPath = $@"{Plugin.filePath}\{modId}\{path}";
        return File.Exists(finalPath);
    }

    public static void CreateDirectory(ScriptedBehaviour script,string path)
    {
        var modId = GetModId(script);
        var finalPath = $@"{Plugin.filePath}\{modId}\{path}";
        Directory.CreateDirectory(finalPath);
    }

    private static ulong GetModId(ScriptedBehaviour script)
    {
        if (script.sourceMutator.sourceMod == null)
        {
            Plugin.Logger.LogWarning($"No associated mod found. Using default folder instead.");
            return 0;
        }
        var modId = script.sourceMutator.sourceMod.workshopItemId.m_PublishedFileId;
        return modId;
    }
}