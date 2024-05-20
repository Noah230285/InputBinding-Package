using UnityEngine;
using System;

namespace UtilEssentials.ScriptableVariables
{
    [Serializable]
    public class ScriptableVariable : ScriptableObject
    {
        [SerializeField] string _description;
    }

    public class VariableReference
    {
        [SerializeField] protected bool _useConstant;
    }
}