using System;
using System.Collections.Generic;
using Ravenfield.SpecOps;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using MoonSharp.Interpreter;

namespace JellyLib.Utilities
{
    /// <summary>
    /// A class that handles the pooling of a given prefab. A pool is only responsible for one prefab at a time.
    /// A destroyed pool does not automatically destroy all pooled instances of its prefab.
    /// </summary>
    public class ObjectPool : IObjectPool<GameObject>
    {
        /// <summary>
        /// The prefab a pool is responsible for.
        /// </summary>
        private GameObject _prefab;
        private readonly Stack<GameObject> _stack = new();
        private bool _initialized;
        
        public bool Initialized => _initialized;

        /// <summary>
        /// Whether you want the pool to automatically set requested objects as active or not.
        /// </summary>
        public bool AutoSetActiveObjects { get; set; } = true;

        public ObjectPool() {}

        public ObjectPool(GameObject prefab, int initialStackSize = 0)
        {
            Initialize(prefab, initialStackSize);
        }

        /// <summary>
        /// Initializes the pool. Attempting to initialize an already initialized pool will result in an error.
        /// </summary>
        /// <param name="prefab">The prefab the pool will be in charge of.</param>
        /// <param name="initialStackSize">The initial stack size of the pool. The pool will instantiate and prepool instances of your prefab based on this number.</param>
        public void Initialize(GameObject prefab, int initialStackSize = 0)
        {
            if (_prefab != null)
            {
                throw new ScriptRuntimeException($"[{nameof(ObjectPool)}.{nameof(Initialize)}]Attempted to initialize a pool that already has an assigned prefab! ({_prefab.name})");
            }
            
            _prefab = prefab;
            for (var i = 0; i < initialStackSize; i++)
            {
                var obj = Object.Instantiate(_prefab);
                Pool(obj);
            }
            
            _initialized = true;
        }

        /// <summary>
        /// Request an object from the pool. If the stack contains no pooled objects, it will instantiate a new instance.
        /// </summary>
        /// <returns>An instance of the pool's assigned prefab.</returns>
        public GameObject RequestObject()
        {
            if (_prefab == null)
            {
                throw new ScriptRuntimeException($"[{nameof(ObjectPool)}.{nameof(RequestObject)}]Attempted to request an object from an uninitialized pool!");
            }
            
            var obj = _stack.Count > 0 ? _stack.Pop() : Object.Instantiate(_prefab);
            obj.SetActive(AutoSetActiveObjects);
            obj.name = _prefab.name + "(Instance)";
            return obj;
        }

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        /// <param name="obj"></param>
        public void Pool(GameObject obj)
        {
            if (_prefab == null)
            {
                throw new ScriptRuntimeException($"[{nameof(ObjectPool)}.{nameof(Pool)}]Attempted to pool an object in an uninitialized pool!");
            }
            
            obj.SetActive(false);
            obj.name = _prefab.name + " (Pooled)";
            _stack.Push(obj);
        }

        /// <summary>
        /// Destroys all pooled instances of the prefab and clears the stack.
        /// </summary>
        public void Clear()
        {
            while (_stack.Count > 0)
            {
                var obj = _stack.Pop();
                Object.Destroy(obj);
            }
            _stack.Clear();
        }

        public int Count()
        {
            return _stack.Count;
        }
    }
}

