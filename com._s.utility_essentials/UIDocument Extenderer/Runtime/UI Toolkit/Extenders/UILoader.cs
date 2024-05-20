using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace UtilEssentials.UIDocumentExtenderer
{
    public class UILoader
    {
        static UILoader _instance;
        static public UILoader instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new();
                    return _instance;
                }
                return _instance;
            }
        }
        public bool isLoading => _operationDictionary == null ? false : _operationDictionary.Count != 0;

        public event Action LoadComplete;
        //public bool loaded => _assetsLoaded >= _assets;
        //int _assets;
        //int _assetsLoaded;

        struct AsyncWrapper
        {
            public AsyncOperationHandle<VisualTreeAsset> Operation;
            public Action<AsyncOperationHandle<VisualTreeAsset>> FinalComplete;
            public void Init(AsyncOperationHandle<VisualTreeAsset> operation, Action<AsyncOperationHandle<VisualTreeAsset>> finalComplete)
            {
                Operation = operation;
                FinalComplete = finalComplete;
                Operation.Completed += FinalComplete;
            }
            public void AddComplete(Action<AsyncOperationHandle<VisualTreeAsset>> action)
            {
                Operation.Completed -= FinalComplete;
                Operation.Completed += action;
                Operation.Completed += FinalComplete;
            }
        }
        Dictionary<string, AsyncWrapper> _operationDictionary = new();


        public void LoadAssetFromKey(string path, Action<AsyncOperationHandle<VisualTreeAsset>> completeAction)
        {
            if (_operationDictionary.ContainsKey(path))
            {
                Debug.Log($"add to path {path}");
                _operationDictionary[path].AddComplete(completeAction);
                return;
            }
            var load = Addressables.LoadAssetAsync<VisualTreeAsset>(path);
            if (load.IsDone)
            {
                Debug.Log($"End early {path}");
                completeAction(load);
                return;
            }
            load.Completed += completeAction;
            AsyncWrapper wrapper = new AsyncWrapper();
            wrapper.Init(load, (operation) =>
            {
                Debug.Log($"load ended {path}");
                _operationDictionary.Remove(path);
                if (_operationDictionary.Count == 0)
                {
                    LoadComplete?.Invoke();
                }
                Debug.Log($"{_operationDictionary.Count}");

            });
            _operationDictionary.Add(path, wrapper);
        }
    }
}