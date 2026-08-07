using UnityEngine;

namespace JellyLib.Utilities
{
    /// <summary>
    /// Class for helping generate unique IDs.
    /// </summary>
    public static class IdHelper
    {
        /// <summary>
        /// Generates a random 64-bit ID.
        /// </summary>
        /// <returns></returns>
        public static ulong Random64()
        {
            var high = (uint) Random.Range(0, int.MaxValue);
            var low = (uint) Random.Range(0, int.MaxValue);
            
            var id = ((ulong)high << 32) | low;
            Plugin.Logger.LogInfo($"Generated id: {id}");
            return id;
        }
    }
}

