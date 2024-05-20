using System;
using UnityEngine;

namespace UtilEssentials.ScriptableVariables
{
    [CreateAssetMenu(menuName = "Util Essentials/Scriptable Variables/Variables/Clamped Integer")]
    [Serializable]
    public class ClampedIntegerVariable : ScriptableVariable
    {
        [SerializeField] IntegerReference _current;
        [SerializeField] IntegerReference _max;

        public int current
        {
            get { return _current.value; }
            set
            {
                _current.value = value;
                _current.value = (int)Mathf.Clamp(_current.value, 0, max);
            }
        }
        public int max { get { return _max.value; } set { _max.value = value; } }

        public float decimalFill => max != 0 ? current / max : 0;
        public float percentFill => decimalFill * 100;
    }

    [Serializable]
    public class ClampedIntegerReference : VariableReference
    {
        [SerializeField] IntegerReference _constantCurrent;
        [SerializeField] IntegerReference _constantMax;
        [SerializeField] ClampedIntegerVariable _variable;
        [SerializeField] ScriptableEventChannel _eventChannel;

        [SerializeField, HideInInspector] bool _configExtended;
        public int current
        {
            get 
            { 
                return _useConstant || _variable == null ? _constantCurrent.value : _variable.current; 
            }
            set
            {
                _eventChannel?.OnRaiseEvents();
                if (_useConstant)
                {
                    _constantCurrent.value = value;
                }
                else
                {
                    _variable.current = value;
                }
            }
        }


        public int max
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

