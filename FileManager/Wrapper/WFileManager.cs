using System.IO;

namespace JellyLib.FileManager.Wrapper;

public static class WFileManager
{
    public static void WriteAllText(string path, string content)
    {
        File.WriteAllText(path, content);
    }

    public static string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    public static bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public static void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }
}