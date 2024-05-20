using System;
using UnityEngine;

namespace UtilEssentials.ScriptableVariables
{
    [CreateAssetMenu(menuName = "Util Essentials/Scriptable Variables/Variables/Clamped Float")]
    [Serializable]
    public class ClampedFloatVariable : ScriptableVariable
    {
        [SerializeField] FloatReference _current;
        [SerializeField] FloatReference _max;

        public float current
        {
            get { return _current.value; }
            set
            {
                _current.value = value;
                _current.value = Mathf.Clamp(_current.value, 0, max);
            }
        }
        public float max { get { return _max.value; } set { _max.value = value; } }

        public float decimalFill => max != 0 ? current / max : 0;
        public float percentFill => decimalFill * 100;
    }

    [Serializable]
    public class ClampedFloatReference : VariableReference
    {
        [SerializeField] FloatReference _constantCurrent;
        [SerializeField] FloatReference _constantMax;
        [SerializeField] ClampedFloatVariable _variable;
        [SerializeField] ScriptableEventChannel _eventChannel;

        [SerializeField, HideInInspector] bool _configExtended;
        public float current
        {
            get 
            { 
                return _useConstant || _variable == null ? _constantCurrent.value : _variable.current; 
            }
            set
            {
                if (_useConstant)
                {
                    if (_constantCurrent.value != value)
                    {
                        _constantCurrent.value = value;
                        _eventChannel?.OnRaiseEvents();
                    }
                }
                else
                {
                    if (_variable.current != value)
                    {
                        _variable.current = value;
                        _eventChannel?.OnRaiseEvents();
                    }
                }
            }
        }


        public float max
        {
            get 
            {
                return _useConstant || _variable == null ? _constantMax.value : _variable.max;
            }
            set
            {
                if (_useConstant)
                {
                    _constantMax.value = value;
                }
                else
                {
                    _variable.max = value;
                }
            }
        }

        public float decimalFill => max != 0 ? current / max : 0;
        public float percentFill => decimalFill * 100;
    }
}

