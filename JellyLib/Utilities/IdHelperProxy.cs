using System;
using Lua.Proxy;
using MoonSharp.Interpreter;

namespace JellyLib.Utilities
{
    [Proxy(typeof(IdHelper))]
    public class IdHelperProxy : IProxy
    {
        [MoonSharpHidden]
        public object GetValue()
        {
            throw new InvalidOperationException("Proxied type is static.");
        }
        
        /// <summary>
        /// Return as string since lua loses precision for ulongs.
        /// </summary>
        /// <returns></returns>
        public static string Random64()
        {
            return IdHelper.Random64().ToString();
        }

    }
}

