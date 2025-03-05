using System;
using Lua.Proxy;
using MoonSharp.Interpreter;
using JellyLib.JsonHelper.Wrapper;

namespace JellyLib.JsonHelper.Proxy;

[Proxy(typeof(WJsonHelper))]
public class JsonHelperProxy
{
    public static string Prettify(string json)
    {
        return WJsonHelper.FormatJson(json);
    }
    
    [MoonSharpHidden]
    public object GetValue()
    {
        throw new InvalidOperationException("Proxied type is static.");
    }
}