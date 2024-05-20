using UnityEngine;
using UnityEngine.Pool;

namespace UtilEssentials.ScriptableVariables
{
    [CreateAssetMenu(menuName = "Util Essentials/Scriptable Variables/Misc/Global Object Pool")]
    public class GlobalObjectPool : ScriptableObject
    {
        [SerializeField] GameObject _pooledPrefab;
        ObjectPool<GameObject> _objectPool = null;
        public ObjectPool<GameObject> objectPool
        {
            get => _objectPool;
        }
        public GameObject Get()
        {
            if (_objectPool == null)
            {
                _objectPool = new ObjectPool<GameObject>(AddToPool, RemoveFromPool, ReturnToPool, DestroyPooled, true, 8, 13);
            }
            return _objectPool.Get();
        }

        public void Release(GameObject obj)
        {
            obj.SetActive(false);
            _objectPool.Release(obj);
        }

        GameObject AddToPool()
        {
            var shot = Instantiate(_pooledPrefab, null);
            return shot;
        }

        void RemoveFromPool(GameObject shot)
        {
            shot.SetActive(true);
        }

        void ReturnToPool(GameObject shot)
        {
            shot.SetActive(false);
            if (_objectPool.CountActive == 0)
            {
                Debug.Log("Reset Pool");
                _objectPool.Dispose();
                _objectPool = null;
            }
        }

        void DestroyPooled(GameObject shot)
        {
            Destroy(shot);
        }
    }
}
