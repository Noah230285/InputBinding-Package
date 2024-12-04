using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UtilEssentials.InputIconBinding
{
    /// <summary>
    /// Customisable data structure based around an input category that contains it's icon path and coresponding icon bind data
    /// </summary>
    [Serializable]
    internal struct IconBindsContainer
    {
        [SerializeField]
        string _nameID;
        public string nameID => _nameID;

        [SerializeField]
        InputBindingCategories _inputBindingCategory;
        public InputBindingCategories inputBindingCategory => _inputBindingCategory;

        [SerializeField]
        List<string> _bindingPaths;

        [SerializeField]
        bool[] _activePlatforms;

        [SerializeField]
        List<string> _tags;

        [SerializeField]
        List<IconBindData> _bindingData;

        /// <summary>
        /// Returns whether the input tags are included in this IconBindsContainer
        /// </summary>
        public bool HasMatchingTags(params string[] checkingTags)
        {
            int matchCount = 0;
            foreach (var checkTag in checkingTags)
            {
                foreach (var thisTag in _tags)
                {
                    if (thisTag.Equals(checkTag))
                    {
                        matchCount++;
                        break;
                    }
                }
            }
            return matchCount == checkingTags.Length;
        }

        /// <summary>
        /// Finds the corresponding IconBindData from a binding path
        /// </summary>
        /// <returns>Whether this container contains the input binding path</returns>
        public bool GetIconBindDataFromBindingPath(string path, out IconBindData data)
        {
            for (int i = 0; i < _bindingPaths.Count; i++)
            {
                if (_bindingPaths[i] == path)
                {
                    data = _bindingData[i];
                    return true;
                }
            }
            data = new();
            return false;
        }

        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="path">The binding path that you are trying to bind</param>
        /// <returns>Whether the binding was successful</returns>
        public bool BindIconAtBindingPathToVisualElement(VisualElement ve, out Action cancelAnimationDelegate, string path)
        {
            if (GetIconBindDataFromBindingPath(path, out IconBindData data))
            {
                return data.BindIconToVisualElement(ve, out cancelAnimationDelegate);
            }
            ve.style.backgroundImage = null;
            cancelAnimationDelegate = null;
            return false;
        }
    }
}