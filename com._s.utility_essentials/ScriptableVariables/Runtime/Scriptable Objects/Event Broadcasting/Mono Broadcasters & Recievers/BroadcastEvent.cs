using UnityEngine;

namespace UtilEssentials.ScriptableVariables.MonoBehaviours
{
    public class BroadcastEvent : MonoBehaviour
    {
        [SerializeField] ScriptableEventChannel channel;

        public void Broadcast()
        {
            channel?.RaiseEvents();
        }
    }
}