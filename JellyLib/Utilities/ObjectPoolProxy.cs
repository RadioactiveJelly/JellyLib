using Lua.Proxy;
using MoonSharp.Interpreter;
using UnityEngine;

namespace JellyLib.Utilities
{
    public class ObjectPoolProxy : IProxy
    {
        [MoonSharpHidden]
        public ObjectPool _value;
    
        [MoonSharpHidden]
        public ObjectPoolProxy(ObjectPool value)
        {
            _value = value;
        }

        public ObjectPoolProxy()
        {
            _value = new ObjectPool();
        }
        
        public ObjectPoolProxy(ObjectPoolProxy source)
        {
            if (source == null)
            {
                throw new ScriptRuntimeException("argument 'source' is nil");
            }
            _value = source._value;
        }

        public ObjectPoolProxy(GameObjectProxy prefab, int initialStackSize = 0)
        {
            if (prefab._value == null)
            {
                throw new ScriptRuntimeException("gameObject cannot be null");
            }
            _value = new ObjectPool(prefab._value, initialStackSize);
        }

        public void Initialize(GameObjectProxy prefab, int initialStackSize = 0)
        {
            if (prefab._value == null)
            {
                throw new ScriptRuntimeException("gameObject cannot be null");
            }
            _value.Initialize(prefab._value, initialStackSize);
        }

        public GameObjectProxy RequestObject()
        {
            return new GameObjectProxy(_value.RequestObject());
        }

        public void Pool(GameObjectProxy obj)
        {
            if (obj._value == null)
            {
                throw new ScriptRuntimeException("gameObject cannot be null");
            }
            
            _value.Pool(obj._value);
        }

        public int Count()
        {
            return _value.Count();
        }

        public object GetValue()
        {
            return _value;
        }
        
        public bool Initialized => _value.Initialized;

        public bool AutoSetActiveObjects
        {
            get => _value.AutoSetActiveObjects;
            set => _value.AutoSetActiveObjects = value;
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static ObjectPoolProxy Call(DynValue _,GameObjectProxy prefab, int initialStackSize = 0)
        {
            return new ObjectPoolProxy(prefab,initialStackSize);
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static ObjectPoolProxy Call(DynValue _, ObjectPoolProxy source)
        {
            return new ObjectPoolProxy(source);
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static ObjectPoolProxy Call(DynValue _)
        {
            return new ObjectPoolProxy();
        }
        
        [MoonSharpHidden]
        public static ObjectPoolProxy New(ObjectPool pool)
        {
            return new ObjectPoolProxy(pool);
        }
    }
}

