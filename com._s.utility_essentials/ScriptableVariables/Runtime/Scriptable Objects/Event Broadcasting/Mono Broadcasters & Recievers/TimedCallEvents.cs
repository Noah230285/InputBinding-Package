using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace UtilEssentials.ScriptableVariables.MonoBehaviours
{
    public class TimedCallEvents : MonoBehaviour
    {
        [SerializeField] UnityEvent _callEvents;

        [SerializeField] float _waitTime;

        void OnEnable()
        {
            StartCoroutine(WaitToCallEvents());
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        public void OnCallEvents()
        {
            _callEvents.Invoke();
        }

        IEnumerator WaitToCallEvents()
        {
            while (enabled)
            {
                yield return new WaitForSeconds(_waitTime);
                OnCallEvents();
            }
        }
    }
}