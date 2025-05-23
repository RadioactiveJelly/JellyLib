﻿using Lua;
using Lua.Proxy;
using MoonSharp.Interpreter;

namespace JellyLib.WeaponUtils
{
    public class WeaponOverrideProxy : IProxy
    {
        [MoonSharpHidden]
        public WeaponOverride _value;

        public WeaponOverrideProxy(WeaponOverride value)
        {
            _value = value;
        }

        public WeaponOverrideProxy()
        {
            _value = new WeaponOverride();
        }
        
        public WeaponOverrideProxy(WeaponOverrideProxy source)
        {
            if (source == null)
            {
                throw new ScriptRuntimeException("argument 'source' is nil");
            }
            _value = source._value;
        }
        
        public int? maxAmmo
        {
            get => _value.maxAmmo;
            set => _value.maxAmmo = value;
        }

        public int? maxSpareAmmo
        {
            get => _value.maxSpareAmmo;
            set => _value.maxSpareAmmo = value;
        }

        public bool autoAdjustAllowedReloads
        {
            get => _value.autoAdjustAllowedReloads;
            set => _value.autoAdjustAllowedReloads = value;
        }

        public int? damage
        {
            get => _value.damage;
            set => _value.damage = value;
        }

        public int? balanceDamage
        {
            get => _value.balanceDamage;
            set => _value.balanceDamage = value;
        }

        public float? kickback
        {
            get => _value.kickback;
            set => _value.kickback = value;
        }
        
        public float? randomKick
        {
            get => _value.randomKick;
            set => _value.randomKick = value;
        }
        
        public float? snapMagnitude
        {
            get => _value.snapMagnitude;
            set => _value.snapMagnitude = value;
        }
        
        public float? snapDuration
        {
            get => _value.snapDuration;
            set => _value.snapDuration = value;
        }
        
        public float? snapFrequency
        {
            get => _value.snapFrequency;
            set => _value.snapFrequency = value;
        }
        
        public float? cooldown
        {
            get => _value.cooldown;
            set => _value.cooldown = value;
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static WeaponOverrideProxy Call(DynValue _)
        {
            return new WeaponOverrideProxy();
        }
        
        [MoonSharpUserDataMetamethod("__call")]
        public static WeaponOverrideProxy Call(DynValue _, WeaponOverrideProxy source)
        {
            return new WeaponOverrideProxy(source);
        }
        
        [MoonSharpHidden]
        public static WeaponOverrideProxy New(WeaponOverride weaponOverride)
        {
            return new WeaponOverrideProxy(weaponOverride);
        }

        [MoonSharpHidden]
        public object GetValue()
        {
            return _value;
        }
    }
}

