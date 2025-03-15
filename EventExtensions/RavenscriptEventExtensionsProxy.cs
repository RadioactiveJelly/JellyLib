using Lua;
using Lua.Proxy;
using MoonSharp.Interpreter;
using System;
using JellyLib.EventExtensions;

namespace JellyLib.EventExtensions.Proxy
{
    [Proxy(typeof(RavenscriptEventExtensions))]
    public class RavenscriptEventExtensionsProxy : IProxy
    {
        [MoonSharpHidden]
        public RavenscriptEventExtensions _value;
        
        [MoonSharpHidden]
        public RavenscriptEventExtensionsProxy(RavenscriptEventExtensions value)
        {
            _value = value;
        }
        public RavenscriptEventExtensionsProxy()
        {
            _value = new();
        }
        
        public ScriptEventProxy onProjectileLandOnTerrain => ScriptEventProxy.New(_value.onProjectileLandOnTerrain);
        public ScriptEventProxy onMedipackResupply => ScriptEventProxy.New(_value.onMedipackResupply);
        public ScriptEventProxy onAmmoBoxResupply => ScriptEventProxy.New(_value.onAmmoBoxResupply);
        public ScriptEventProxy onPlayerFireWeapon => ScriptEventProxy.New(_value.onPlayerFireWeapon);
        
        [MoonSharpHidden]
        public object GetValue()
        {
            return _value;
        }
        
        [MoonSharpHidden]
        public static RavenscriptEventExtensionsProxy New(RavenscriptEventExtensions value)
        {
            if (value == null)
            {
                return null;
            }
            RavenscriptEventExtensionsProxy ravenscriptEventsExtensionsProxy = (RavenscriptEventExtensionsProxy)ObjectCache.Get(typeof(RavenscriptEventExtensions), value);
            if (ravenscriptEventsExtensionsProxy == null)
            {
                ravenscriptEventsExtensionsProxy = new RavenscriptEventExtensionsProxy(value);
                ObjectCache.Add(typeof(RavenscriptEventExtensionsProxy), value, ravenscriptEventsExtensionsProxy);
            }
            return ravenscriptEventsExtensionsProxy;
        }

        [MoonSharpUserDataMetamethod("__call")]
        public static RavenscriptEventExtensionsProxy Call(DynValue _)
        {
            return new RavenscriptEventExtensionsProxy();
        }
    }
}

