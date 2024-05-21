using UnityEngine;
using UnityEngine.Events;

namespace UtilEssentials.ScriptableVariables.MonoBehaviours
{
    public class AddToBroadcast : MonoBehaviour
    {
        [SerializeField] ScriptableEventChannel _channel;
        public UnityEvent CalledEvents;

        private void OnEnable()
        {
            _channel.RaiseEvents += OnCalledEvents;
        }

        private void OnDisable()
        {
            _channel.RaiseEvents -= OnCalledEvents;
        }

        public void OnCalledEvents()
        {
            CalledEvents.Invoke();
        }
    }
}