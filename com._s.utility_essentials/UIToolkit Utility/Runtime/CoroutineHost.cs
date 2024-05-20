using System;
using System.Collections;
using UnityEngine;

namespace UtilEssentials.UIToolkitUtility
{
    public class CoroutineHost : MonoBehaviour
    {
        static CoroutineHost _instance;
        public static CoroutineHost instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindExisting() ?? CreateNew();
                }
                return _instance;
            }
        }

        static CoroutineHost FindExisting()
        {
            var hosts = FindObjectsOfType<CoroutineHost>();

            if (hosts == null || hosts.Length == 0) return null;

            return hosts[0];
        }

        static CoroutineHost CreateNew()
        {
            var newHost = new GameObject("Coroutine Host Singleton");
            return newHost.AddComponent<CoroutineHost>();
        }

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }

        public void EndOfFrameAction(Action action)
        {
            IEnumerator Coroutine()
            {
                yield return new WaitForEndOfFrame();
                action();
            }
            StartCoroutine(Coroutine());
        }
    }
}
