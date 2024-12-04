using System;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace UtilEssentials.InputIconBinding
{
    [Serializable, DefaultExecutionOrder(-1)]
    public class ButtonIconBindingsEditorSettings : ScriptableObject
    {
        internal static ButtonIconBindingsEditorSettings _instance;
        public static ButtonIconBindingsEditorSettings instance
        {
            get
            {
                if (_instance == null)
                {

                    ButtonIconBindingsEditorSettings loaded = Resources.Load<ButtonIconBindingsEditorSettings>("ButtonIconBindingEditorSettings/ButtonIconBindingEditorSettings");
                    if (loaded == null)
                    {
#if UNITY_EDITOR
                        loaded = MyCustomSettingsIMGUIRegister.CreateNewConfig();
#endif
                    }
                    _instance = loaded;
                }
                return _instance;
            }
        }

        [SerializeField]
        internal SOButtonIconBindings _selectedBindings;

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="category">Attempts to bind the first IconBindsContainer with this category</param>
        /// <param name="tags">Attempts to bind the first IconBindsContainer with these tags</param>
        /// <returns>Whether the binding was successful</returns>
        public static bool BindVisualElementImageToIconBinding(VisualElement ve, out Action cancelAnimationDelegate, string bindingPath, string nameID, InputBindingCategories category, string[] tags, InputBindingSearchType searchType)
        {
            switch (searchType)
            {
                case InputBindingSearchType.Category:
                    return BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, category);
                case InputBindingSearchType.NameID:
                    return BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, nameID);
                case InputBindingSearchType.Tags:
                    return BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, null, tags);
                case InputBindingSearchType.CategoryAndNameID:
                    return BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, category, nameID);
                case InputBindingSearchType.NameIDAndTags:
                    return BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, nameID, tags);
                case InputBindingSearchType.CategoryAndTags:
                    return BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, null, category, tags);
                case InputBindingSearchType.CategoryAndNameIDAndTags:
                    return BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, nameID, category, tags);
                default:
                    cancelAnimationDelegate = null;
                    return false;
            }
        }
        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="category">Attempts to bind the first IconBindsContainer with this category</param>
        /// <param name="tags">Attempts to bind the first IconBindsContainer with these tags</param>
        /// <returns>Whether the binding was successful</returns>
        public static bool BindVisualElementImageToIconBinding(VisualElement ve, out Action cancelAnimationDelegate, string bindingPath, string NameID, InputBindingCategories category, params string[] tags)
        {
            if (NameID == null)
            {
                return BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, category, tags);
            }
            else
            {
                cancelAnimationDelegate = null;
                if (tags == null)
                {
                    return instance?._selectedBindings.BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, NameID, category) ?? false;
                }
                else
                {
                    return instance?._selectedBindings.BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, NameID, category, tags) ?? false;
                }
            }
        }

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="category">Attempts to bind the first IconBindsContainer with this category</param>
        /// <param name="tags">Attempts to bind the first IconBindsContainer with these tags</param>
        /// <returns>Whether the binding was successful</returns>
        public static bool BindVisualElementImageToIconBinding(VisualElement ve, out Action cancelAnimationDelegate, string bindingPath, InputBindingCategories category, params string[] tags)
        {
            cancelAnimationDelegate = null;
            if (tags != null)
            {
                return instance?._selectedBindings.BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, category, tags) ?? false;
            }
            else
            {
                return instance?._selectedBindings.BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, category) ?? false;
            }
        }

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="nameID">Attempts to bind the first IconBindsContainer with this nameID</param>
        /// <returns>Whether the binding was successful</returns>
        public static bool BindVisualElementImageToIconBinding(VisualElement ve, out Action cancelAnimationDelegate, string bindingPath, string nameID)
        {
            cancelAnimationDelegate = null;
            return instance?._selectedBindings.BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, nameID) ?? false;
        }

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="nameID">Attempts to bind the first IconBindsContainer with this nameID, if you want to ignore the nameID set it to null</param>
        /// <param name="tags">Attempts to bind the first IconBindsContainer with these tags</param>
        /// <returns>Whether the binding was successful</returns>
        public static bool BindVisualElementImageToIconBinding(VisualElement ve, out Action cancelAnimationDelegate, string bindingPath, string nameID, params string[] tags)
        {
            cancelAnimationDelegate = null;
            if (nameID != null)
            {
                if (tags != null)
                {
                    return instance?._selectedBindings.BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, nameID, tags) ?? false;
                }
                else
                {
                    return instance?._selectedBindings.BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, nameID) ?? false;
                }
            }
            else
            {
                return instance?._selectedBindings.BindVisualElementImageToIconBinding(ve, out cancelAnimationDelegate, bindingPath, tags) ?? false;
            }
        }



        /// <summary>
        /// Gets the icon binding data of a input action from its binding path
        /// </summary>
        /// <param name="outIconBindData">Returns the IconBindData that was found, if there was none found it will be empty</param>
        /// <param name="bindingPath">The binding path that you are trying to get the data for</param>
        /// <param name="category">Attempts to get the first IconBindsContainer with this category</param>
        /// <param name="tags">Attempts to get the first IconBindsContainer with these tags</param>
        /// <returns>Whether the an icon binding for the input binding path could be found</returns>
        public static bool GetIconBindData(out IconBindData outIconBindData, string bindingPath, string nameID, InputBindingCategories category, string[] tags, InputBindingSearchType searchType)
        {
            switch (searchType)
            {
                case InputBindingSearchType.None:
                    return GetIconBindData(out outIconBindData, bindingPath);
                case InputBindingSearchType.Category:
                    return GetIconBindData(out outIconBindData, bindingPath, category);
                case InputBindingSearchType.NameID:
                    return GetIconBindData(out outIconBindData, bindingPath, nameID);
                case InputBindingSearchType.Tags:
                    return GetIconBindData(out outIconBindData, bindingPath, null, tags);
                case InputBindingSearchType.CategoryAndNameID:
                    return GetIconBindData(out outIconBindData, bindingPath, category, nameID);
                case InputBindingSearchType.NameIDAndTags:
                    return GetIconBindData(out outIconBindData, bindingPath, nameID, tags);
                case InputBindingSearchType.CategoryAndTags:
                    return GetIconBindData(out outIconBindData, bindingPath, null, category, tags);
                case InputBindingSearchType.CategoryAndNameIDAndTags:
                    return GetIconBindData(out outIconBindData, bindingPath, nameID, category, tags);
                default:
                    outIconBindData = new();
                    return false;
            }
        }
        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="outIconBindData">Returns the IconBindData that was found, if there was none found it will be empty</param>
        /// <param name="bindingPath">The binding path that you are trying to get the data for</param>
        /// <param name="category">Attempts to get the first IconBindsContainer with this category</param>
        /// <param name="tags">Attempts to get the first IconBindsContainer with these tags</param>
        /// <returns>Whether the an icon binding for the input binding path could be found</returns>
        public static bool GetIconBindData(out IconBindData outIconBindData, string bindingPath, string NameID, InputBindingCategories category, params string[] tags)
        {
            if (NameID == null)
            {
                return GetIconBindData(out outIconBindData, bindingPath, category, tags);
            }
            else
            {
                outIconBindData = new();

                if (tags == null)
                {
                    return instance?._selectedBindings.GetIconBindData(out outIconBindData, bindingPath, NameID, category) ?? false;
                }
                else
                {
                    return instance?._selectedBindings.GetIconBindData(out outIconBindData, bindingPath, NameID, category, tags) ?? false;
                }
            }
        }

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="outIconBindData">Returns the IconBindData that was found, if there was none found it will be empty</param>
        /// <param name="bindingPath">The binding path that you are trying to get the data for</param>
        /// <param name="category">Attempts to get the first IconBindsContainer with this category</param>
        /// <param name="tags">Attempts to get the first IconBindsContainer with these tags</param>
        /// <returns>Whether the an icon binding for the input binding path could be found</returns>
        public static bool GetIconBindData(out IconBindData outIconBindData, string bindingPath, InputBindingCategories category, params string[] tags)
        {
            outIconBindData = new();
            if (tags != null)
            {
                return instance?._selectedBindings.GetIconBindData(out outIconBindData, bindingPath, category, tags) ?? false;
            }
            else
            {
                return instance?._selectedBindings.GetIconBindData(out outIconBindData, bindingPath, category) ?? false;
            }
        }

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="outIconBindData">Returns the IconBindData that was found, if there was none found it will be empty</param>
        /// <param name="bindingPath">The binding path that you are trying to get the data for</param>
        /// <param name="nameID">Attempts to get the first IconBindsContainer with this nameID</param>
        /// <returns>Whether the an icon binding for the input binding path could be found</returns>
        public static bool GetIconBindData(out IconBindData outIconBindData, string bindingPath, string nameID)
        {
            outIconBindData = new();
            return instance?._selectedBindings.GetIconBindData(out outIconBindData, bindingPath, nameID) ?? false;
        }

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="outIconBindData">Returns the IconBindData that was found, if there was none found it will be empty</param>
        /// <param name="bindingPath">The binding path that you are trying to get the data for</param>
        /// <param name="nameID">Attempts to get the first IconBindsContainer with this nameID, if you want to ignore the nameID set it to null</param>
        /// <param name="tags">Attempts to get the first IconBindsContainer with these tags</param>
        /// <returns>Whether the an icon binding for the input binding path could be found</returns>
        public static bool GetIconBindData(out IconBindData outIconBindData, string bindingPath, string nameID, params string[] tags)
        {
            outIconBindData = new();
            if (nameID != null)
            {
                if (tags != null)
                {
                    return instance?._selectedBindings.GetIconBindData(out outIconBindData, bindingPath, nameID, tags) ?? false;
                }
                else
                {
                    return instance?._selectedBindings.GetIconBindData(out outIconBindData, bindingPath, nameID) ?? false;
                }
            }
            else
            {
                return instance?._selectedBindings.GetIconBindData(out outIconBindData, bindingPath, tags) ?? false;
            }
        }

        public static bool GetIconBindData(out IconBindData outIconBindData, string bindingPath)
        {
            outIconBindData = new();
            return instance?._selectedBindings.GetIconBindData(out outIconBindData, bindingPath) ?? false;
        }
    }

#if UNITY_EDITOR

    // Register a SettingsProvider using IMGUI for the drawing framework:
    internal static class MyCustomSettingsIMGUIRegister
    {

        public const string k_MyCustomSettingsPath = "/ProjectSettings/ButtonIconBindingSettings.asset";
        public static string path => Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length) + k_MyCustomSettingsPath;

        internal static ButtonIconBindingsEditorSettings CreateNewConfig()
        {

            ButtonIconBindingsEditorSettings settings = ScriptableObject.CreateInstance<ButtonIconBindingsEditorSettings>();

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources/ButtonIconBindingEditorSettings"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "ButtonIconBindingEditorSettings");
            }
            AssetDatabase.CreateAsset(settings, "Assets/Resources/ButtonIconBindingEditorSettings/ButtonIconBindingEditorSettings.asset");
            AssetDatabase.SaveAssets();
            ButtonIconBindingsEditorSettings._instance = settings;
            return settings;
        }


        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new SettingsProvider("Project/ButtonIconBindingsEditorSettingsMenu", SettingsScope.Project)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = "Button Icon Binding",
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) =>
                {
                    var settings = ButtonIconBindingsEditorSettings.instance;
                    SerializedObject serializedObject = new SerializedObject(settings);
                    EditorGUILayout.ObjectField(serializedObject.FindProperty("_selectedBindings"), new GUIContent("Selected Binding Settings"));
                    serializedObject.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Number", "Some String" })
            };

            return provider;
        }
    }
#endif
}