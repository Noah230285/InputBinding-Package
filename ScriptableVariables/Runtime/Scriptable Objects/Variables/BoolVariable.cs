using System;
using UnityEngine;

namespace UtilEssentials.ScriptableVariables
{
    [CreateAssetMenu(menuName = "Util Essentials/Scriptable Variables/Variables/Bool")]
    [Serializable]
    public class BoolVariable : ScriptableVariable
    {
        public bool Value;
    }

    [Serializable]
    public class BoolReference : VariableReference
    {
        [SerializeField] bool _constantValue;
        [SerializeField] BoolVariable _variable;

        public bool value
        {
            get { return _useConstant || _variable == null? _constantValue : _variable.Value; }
        }
    }
}