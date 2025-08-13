using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using BepInEx;
using Lua.Proxy;
using MoonSharp.Interpreter;

namespace JellyLib.Utilities
{
    [Proxy(typeof(Stopwatch))]
    public class StopwatchProxy
    {
        [MoonSharpHidden]
        public Stopwatch _value;
        
        [MoonSharpHidden]
        public StopwatchProxy(Stopwatch value)
        {
            _value = value;
        }
        
        public StopwatchProxy()
        {
            _value = new Stopwatch();
        }

        public StopwatchProxy(StopwatchProxy source)
        {
            if (source == null)
            {
                throw new ScriptRuntimeException("argument 'source' is nil");
            }
            _value = source._value;
        }

        public void Start()
        {
            _value.Start();
        }

        public void Stop()
        {
            _value.Stop();
        }

        public void Log(string context)
        {
            const string messageFormat = "{0}: Operation took {1}{2}ms{3}";

            var elapsedMilliseconds = _value.ElapsedMilliseconds;
            var prefix = elapsedMilliseconds switch
            {
                >= 2000 => "<color=red>",
                >= 1000 => "<color=orange>",
                >= 500 => "<color=yellow>",
                _ => "<color=green>"
            };

            var message = string.Format(messageFormat, context, prefix, elapsedMilliseconds, "</color>");
            Lua.ScriptConsole.instance.LogInfo(message);
        }
        
        [MoonSharpHidden]
        public static StopwatchProxy New(Stopwatch stopwatch)
        {
            return new StopwatchProxy(stopwatch);
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static StopwatchProxy Call(DynValue _, StopwatchProxy source)
        {
            return new StopwatchProxy(source);
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static StopwatchProxy Call(DynValue _)
        {
            return new StopwatchProxy();
        }
        
        public long ElapsedMilliseconds => _value.ElapsedMilliseconds; 
    }
}

