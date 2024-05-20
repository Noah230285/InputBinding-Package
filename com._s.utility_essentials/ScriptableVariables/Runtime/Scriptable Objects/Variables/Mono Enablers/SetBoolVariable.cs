using UnityEngine;

namespace UtilEssentials.ScriptableVariables.MonoBehaviours
{
    public class SetBoolVariable : MonoBehaviour
    {
        [SerializeField] BoolVariable[] _bools;
        [SerializeField] ScriptableEventChannel _changeEvent;
        [SerializeField] bool _setOnStart;
        [SerializeField] bool _default;

        void Start()
        {
            if (_setOnStart)
            {
                for (int i = 0; i < _bools.Length; i++)
                {
                    _bools[i].Value = _default;
                }
            }
        }

        public void BoolSet(bool setValue)
        {
            for (int i = 0; i < _bools.Length; i++)
            {
                _bools[i].Value = setValue;
            }
            _changeEvent?.OnRaiseEvents();
        }

        public bool GetBoolValue(int i = 0)
        {
            return _bools[i].Value;
        }
    }
}
