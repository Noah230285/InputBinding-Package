using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UtilEssentials.InputIconBinding
{
    [CreateAssetMenu(menuName = "Util Essentials/InputAction Binding/Button Icon Bindings")]
    public class SOButtonIconBindings : ScriptableObject
    {
        [SerializeField] List<IconBindsContainer> _iconBindsContainersList;

        public bool GetIconBindData(out IconBindData outIconBindData, string bindingPath)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.GetIconBindDataFromBindingPath(bindingPath, out IconBindData data))
                {
                    outIconBindData = data;
                    return true;
                }
            }
            outIconBindData = new();
            return false;
        }

        public bool GetIconBindData(out IconBindData outIconBindData, string bindingPath, InputBindingCategories category)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.inputBindingCategory == category && set.GetIconBindDataFromBindingPath(bindingPath, out IconBindData data))
                {
                    outIconBindData = data;
                    return true;
                }
            }
            outIconBindData = new();
            return false;
        }

        public bool GetIconBindData(out IconBindData outIconBindData, string bindingPath, string nameID)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.nameID == nameID && set.GetIconBindDataFromBindingPath(bindingPath, out IconBindData data))
                {
                    outIconBindData = data;
                    return true;
                }
            }
            outIconBindData = new();
            return false;
        }

        public bool GetIconBindData(out IconBindData outIconBindData, string bindingPath, params string[] tags)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.HasMatchingTags(tags) && set.GetIconBindDataFromBindingPath(bindingPath, out IconBindData data))
                {
                    outIconBindData = data;
                    return true;
                }
            }
            outIconBindData = new();
            return false;
        }

        public bool GetIconBindData(out IconBindData outIconBindData, string bindingPath, string nameID, InputBindingCategories category)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.nameID == nameID && set.inputBindingCategory == category && set.GetIconBindDataFromBindingPath(bindingPath, out IconBindData data))
                {
                    outIconBindData = data;
                    return true;
                }
            }
            outIconBindData = new();
            return false;
        }

        public bool GetIconBindData(out IconBindData outIconBindData, string bindingPath, InputBindingCategories category, params string[] tags)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.inputBindingCategory == category && set.HasMatchingTags(tags) && set.GetIconBindDataFromBindingPath(bindingPath, out IconBindData data))
                {
                    outIconBindData = data;
                    return true;
                }
            }
            outIconBindData = new();
            return false;
        }

        public bool GetIconBindData(out IconBindData outIconBindData, string bindingPath, string nameID, params string[] tags)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.nameID == nameID && set.HasMatchingTags(tags) && set.GetIconBindDataFromBindingPath(bindingPath, out IconBindData data))
                {
                    outIconBindData = data;
                    return true;
                }
            }
            outIconBindData = new();
            return false;
        }

        public bool GetIconBindData(out IconBindData outIconBindData, string bindingPath, string nameID, InputBindingCategories category, params string[] tags)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.nameID == nameID && set.inputBindingCategory == category && set.HasMatchingTags(tags) && set.GetIconBindDataFromBindingPath(bindingPath, out IconBindData data))
                {
                    outIconBindData = data;
                    return true;
                }
            }
            outIconBindData = new();
            return false;
        }


        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="category">Attempts to bind the first IconBindsContainer with this category</param>
        /// <returns>Whether the binding was successful</returns>
        public bool BindVisualElementImageToIconBinding(VisualElement ve, out Action cancelAnimationDelegate, string bindingPath, InputBindingCategories category)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.inputBindingCategory == category)
                {
                    return set.BindIconAtBindingPathToVisualElement(ve, out cancelAnimationDelegate, bindingPath);
                }
            }
            ve.style.backgroundImage = null;
            cancelAnimationDelegate = null;
            return false;
        }

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="nameID">Attempts to bind the first IconBindsContainer with this nameID</param>
        /// <returns>Whether the binding was successful</returns>
        public bool BindVisualElementImageToIconBinding(VisualElement ve, out Action cancelAnimationDelegate, string bindingPath, string nameID)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.nameID.Equals(nameID))
                {
                    return set.BindIconAtBindingPathToVisualElement(ve, out cancelAnimationDelegate, bindingPath);
                }
            }
            ve.style.backgroundImage = null;
            cancelAnimationDelegate = null;
            return false;
        }

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="tags">Attempts to bind the first IconBindsContainer with these tags</param>
        /// <returns>Whether the binding was successful</returns>
        public bool BindVisualElementImageToIconBinding(VisualElement ve, out Action cancelAnimationDelegate, string bindingPath, params string[] tags)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.HasMatchingTags(tags))
                {
                    return set.BindIconAtBindingPathToVisualElement(ve, out cancelAnimationDelegate, bindingPath);
                }
            }
            ve.style.backgroundImage = null;
            cancelAnimationDelegate = null;
            return false;
        }

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="nameID">Attempts to bind the first IconBindsContainer with this nameID</param>
        /// <param name="category">Searches the first IconBindsContainer with these tags</param>
        /// <returns>Whether the binding was successful</returns>
        public bool BindVisualElementImageToIconBinding(VisualElement ve, out Action cancelAnimationDelegate, string bindingPath, string nameID, InputBindingCategories category)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.nameID.Equals(nameID) && set.inputBindingCategory == category)
                {
                    return set.BindIconAtBindingPathToVisualElement(ve, out cancelAnimationDelegate, bindingPath);
                }
            }
            ve.style.backgroundImage = null;
            cancelAnimationDelegate = null;
            return false;
        }

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="category">Searches the first IconBindsContainer with these tags</param>
        /// <param name="tags">Searches the first IconBindsContainer with these tags</param>
        /// <returns>Whether the binding was successful</returns>
        public bool BindVisualElementImageToIconBinding(VisualElement ve, out Action cancelAnimationDelegate, string bindingPath, InputBindingCategories category, params string[] tags)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.inputBindingCategory == category && set.HasMatchingTags(tags))
                {
                    return set.BindIconAtBindingPathToVisualElement(ve, out cancelAnimationDelegate, bindingPath);
                }
            }
            ve.style.backgroundImage = null;
            cancelAnimationDelegate = null;
            return false;
        }
        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="nameID">Attempts to bind the first IconBindsContainer with this nameID</param>
        /// <param name="tags">Searches the first IconBindsContainer with these tags</param>
        /// <returns>Whether the binding was successful</returns>
        public bool BindVisualElementImageToIconBinding(VisualElement ve, out Action cancelAnimationDelegate, string bindingPath, string nameID, params string[] tags)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.nameID.Equals(nameID) && set.HasMatchingTags(tags))
                {
                    return set.BindIconAtBindingPathToVisualElement(ve, out cancelAnimationDelegate, bindingPath);
                }
            }
            ve.style.backgroundImage = null;
            cancelAnimationDelegate = null;
            return false;
        }

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="nameID">Attempts to bind the first IconBindsContainer with this nameID</param>
        /// <param name="category">Searches the first IconBindsContainer with these tags</param>
        /// <param name="tags">Searches the first IconBindsContainer with these tags</param>
        /// <returns>Whether the binding was successful</returns>
        public bool BindVisualElementImageToIconBinding(VisualElement ve, out Action cancelAnimationDelegate, string bindingPath, string nameID, InputBindingCategories category, params string[] tags)
        {
            foreach (var set in _iconBindsContainersList)
            {
                if (set.nameID.Equals(nameID) && set.inputBindingCategory == category && set.HasMatchingTags(tags))
                {
                    return set.BindIconAtBindingPathToVisualElement(ve, out cancelAnimationDelegate, bindingPath);
                }
            }
            ve.style.backgroundImage = null;
            cancelAnimationDelegate = null;
            return false;
        }
    }
}
