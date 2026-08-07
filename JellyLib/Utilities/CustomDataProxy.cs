using Lua.Proxy;
using MoonSharp.Interpreter;

namespace JellyLib.Utilities
{
    [Proxy(typeof(CustomData))]
    public class CustomDataProxy : IProxy
    {
        [MoonSharpHidden]
        public CustomData _value;
        
        [MoonSharpHidden]
        public object GetValue()
        {
            return _value;
        }
        
        public CustomDataProxy(CustomData value)
        {
            _value = value;
        }
        
        public CustomDataProxy()
        {
            _value = new CustomData();
        }
        
        public CustomDataProxy(CustomDataProxy source)
        {
            if (source == null)
            {
                throw new ScriptRuntimeException("argument 'source' is nil");
            }
            _value = source._value;
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static CustomDataProxy Call(DynValue _)
        {
            return new CustomDataProxy();
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static CustomDataProxy Call(DynValue _, CustomDataProxy source)
        {
            return new CustomDataProxy(source);
        }
        
        [MoonSharpHidden]
        public static CustomDataProxy New(CustomData customData)
        {
            return new CustomDataProxy(customData);
        }

        public void SetFloat(string key, float value)
        {
            _value.SetFloat(key, value);
        }

        public float GetFloat(string key)
        {
            return _value.GetFloat(key);
        }

        public bool HasFloat(string key)
        {
            return _value.HasFloat(key);
        }

        public void SetInt(string key, int value)
        {
            _value.SetInt(key, value);
        }

        public int GetInt(string key)
        {
            return _value.GetInt(key);
        }

        public bool HasInt(string key)
        {
            return _value.HasInt(key);
        }

        public void SetBool(string key, bool value)
        {
            _value.SetBoolean(key, value);
        }

        public bool GetBool(string key)
        {
            return _value.GetBoolean(key);
        }

        public bool HasBool(string key)
        {
            return _value.HasBoolean(key);
        }

        public void SetString(string key, string value)
        {
            _value.SetString(key, value);
        }

        public string GetString(string key)
        {
            return _value.GetString(key);
        }

        public bool HasString(string key)
        {
            return _value.HasString(key);
        }
    }
}

