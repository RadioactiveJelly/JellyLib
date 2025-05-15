using System;
using Lua;
using Lua.Proxy;
using MoonSharp.Interpreter;
using Ravenfield.SpecOps;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

namespace JellyLib.GameModeUtils
{
    [Proxy(typeof(SpecOpsObjective))]
    public class ObjectiveProxy : IProxy
    {
        public SpecOpsObjective _value;

        public ObjectiveProxy()
        {
            _value = new SpecOpsObjective();
        }
        
        public ObjectiveProxy(SpecOpsObjective value)
        {
            _value = value;
        }

        public ObjectiveProxy(ObjectiveProxy source)
        {
            if (source == null)
            {
                throw new ScriptRuntimeException("argument 'source' is nil");
            }
            _value = source._value;
        }
        
        public string GetObjectiveName()
        {
            return _value.objective.text.text;
        }

        public Vector3 GetObjectivePosition()
        {
            return _value.objective.GetTargetWorldPosition();
        }

        public bool IsDestroyObjective()
        {
            if (_value is not SpecOpsScenario scenario)
                return false;
            
            return scenario is DestroyScenario;
        }

        public bool IsPatrolObjective()
        {
            return _value is SpecOpsPatrol;
        }

        public Vector3 GetPatrolPosition()
        {
            if(_value is not SpecOpsPatrol patrol)
                return _value.objective.GetTargetWorldPosition();

            if(patrol.squad == null)
                return _value.objective.GetTargetWorldPosition();
            
            if(patrol.squad.Leader() == null)
                return _value.objective.GetTargetWorldPosition();

            if(patrol.squad.Leader().actor == null)
                return _value.objective.GetTargetWorldPosition();

            return patrol.squad.Leader().actor.CenterPosition();
        }
        
        public Transform GetTargetVehicleTransform()
        {
            if (_value is not DestroyScenario)
                return null;
            
            var destroyScenario = _value as DestroyScenario;
            FieldInfo vehicleField = typeof(DestroyScenario).GetField("targetVehicle", BindingFlags.NonPublic | BindingFlags.Instance);
            if (vehicleField == null)
                return null;
            
            var vehicle = vehicleField.GetValue(destroyScenario) as Vehicle;
            if (vehicle == null)
                return null;

            return vehicle.transform;
        }

        public Transform GetObjectiveTransform()
        {
            return _value.objective.targetTransform;
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static ObjectiveProxy Call(DynValue _)
        {
            return new ObjectiveProxy();
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static ObjectiveProxy Call(DynValue _, ObjectiveProxy source)
        {
            return new ObjectiveProxy(source);
        }
        
        [MoonSharpHidden]
        public static ObjectiveProxy New(SpecOpsObjective weaponOverride)
        {
            return new ObjectiveProxy(weaponOverride);
        }
        
        [MoonSharpHidden]
        public object GetValue()
        {
            return _value;
        }
    }
}

