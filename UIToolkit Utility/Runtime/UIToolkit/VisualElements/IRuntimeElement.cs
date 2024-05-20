using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using UtilEssentials.UIDocumentExtenderer;

#if UNITY_EDITOR
using UtilEssentials.UIToolkitUtility.Editor;
#endif

namespace UtilEssentials.UIToolkitUtility.VisualElements
{
    public interface IRuntimeElement
    {
        public UnityAction loadFinished { get; set; }
        public bool loaded { get; set; }
        public void LoadAssets(string address, string packageName = null)
        {
#if UNITY_EDITOR
            if (packageName == null)
            {
                return;
            }
            //Find the path for this package
            string assetPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            string beginingPath = UIToolkitUtilityFunctions.GetBeginningOfPackagePath(assetPath, packageName);

            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            $"{beginingPath}{address}");

            asset.CloneTree(this as VisualElement);
            loaded = true;
        loadFinished?.Invoke();
#else
            Debug.Log("Load Request");
            UILoader.instance.LoadAssetFromKey(address, OnLoadFinished);
            //var asset = UILoader..LoadAssetAsync<VisualTreeAsset>(address);
            //    asset.Completed += OnLoadFinished;
#endif
        }
#if !UNITY_EDITOR
        void OnLoadFinished(AsyncOperationHandle<VisualTreeAsset> x)
        {
            Debug.Log("Loaded");
            var root = this as VisualElement;
            int count = root.childCount;
            VisualElement childContainer = new();
            for (int i = 0; i < count; i++)
            {
                childContainer.Add(root.ElementAt(0));
            }
            x.Result.CloneTree(root);
            root.Add(childContainer);
            loaded = true;
            loadFinished?.Invoke();
        }
#endif
    }
}
