using System;
using UnityEngine;

namespace UtilEssentials.ScriptableVariables
{
    [CreateAssetMenu(menuName = "Util Essentials/Scriptable Variables/Variables/Float")]
    [Serializable]
    public class FloatVariable : ScriptableVariable
    {
        public float Value;
    }

    [Serializable]
    public class FloatReference : VariableReference
    {
        [SerializeField] float _constantValue;
        [SerializeField] FloatVariable _variable;

        public float value
        {
            get { return _useConstant || _variable == null ? _constantValue : _variable.Value; }
            set
            {
                if (_useConstant)
                {
                    _constantValue = value;
                }
                else
                {
                    _variable.Value = value;
                }
            }
        }
    }
}