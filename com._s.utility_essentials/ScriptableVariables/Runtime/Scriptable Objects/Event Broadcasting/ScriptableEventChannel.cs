using UnityEngine;
using UnityEngine.Events;

namespace UtilEssentials.ScriptableVariables
{
    [CreateAssetMenu(menuName = "Util Essentials/Scriptable Variables/Misc/Event Channel")]
    public class ScriptableEventChannel : ScriptableObject
    {
        public UnityAction RaiseEvents;

        public void OnRaiseEvents()
        {
            RaiseEvents?.Invoke();
        }
    }
}
