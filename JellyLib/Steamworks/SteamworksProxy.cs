using System;
using System.Diagnostics;
using Steamworks;
using UnityEngine;
using Lua;
using Lua.Proxy;
using MoonSharp.Interpreter;

namespace JellyLib.Steamworks
{
    [Proxy(typeof(SteamworksExtension))]
    public class SteamworksProxy : IProxy
    {
        [MoonSharpHidden]
        public object GetValue()
        {
            throw new InvalidOperationException("Proxied type is static.");
        }
        
        public static TextureProxy GetUserProfileImage()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var user = SteamUser.GetSteamID();
            var avatar = SteamFriends.GetMediumFriendAvatar(user);
            var avatarTexture = SteamworksExtension.GetSteamImageAsTexture2D(avatar);

            stopwatch.Stop();
            Plugin.Logger.LogInfo($"[{nameof(SteamworksProxy)}.{nameof(GetUserProfileImage)}]Operation took {stopwatch.ElapsedMilliseconds}ms]");
            return new TextureProxy(avatarTexture);
        }
    }
}

