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

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="category">Attempts to bind the first IconBindsContainer with this category</param>
        /// <param name="tags">Attempts to bind the first IconBindsContainer with these tags</param>
        /// <returns>Whether the binding was successful</returns>
        public static bool BindVisualElementImageToIconBinding(VisualElement ve, string bindingPath, InputBindingCategories category, params string[] tags)
        {
            if (tags != null)
            {
                return instance?._selectedBindings.BindVisualElementImageToIconBinding(ve, bindingPath, category, tags) ?? false;
            }
            else
            {
                return instance?._selectedBindings.BindVisualElementImageToIconBinding(ve, bindingPath, category) ?? false;
            }
        }

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="nameID">Attempts to bind the first IconBindsContainer with this nameID</param>
        /// <returns>Whether the binding was successful</returns>
        public static bool BindVisualElementImageToIconBinding(VisualElement ve, string bindingPath, string nameID)
        {
            return instance?._selectedBindings.BindVisualElementImageToIconBinding(ve, bindingPath, nameID) ?? false;
        }

        /// <summary>
        /// Binds the background image of a VisualElement to the icon image/animation at bindingPath
        /// </summary>
        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <param name="bindingPath">The binding path that you are trying to bind</param>
        /// <param name="nameID">Attempts to bind the first IconBindsContainer with this nameID, if you want to ignore the nameID set it to null</param>
        /// <param name="tags">Attempts to bind the first IconBindsContainer with these tags</param>
        /// <returns>Whether the binding was successful</returns>
        public static bool BindVisualElementImageToIconBinding(VisualElement ve, string bindingPath, string nameID, params string[] tags)
        {
            if (nameID != null)
            {
                if (tags != null)
                {
                    return instance?._selectedBindings.BindVisualElementImageToIconBinding(ve, bindingPath, nameID, tags) ?? false;
                }
                else
                {
                    return instance?._selectedBindings.BindVisualElementImageToIconBinding(ve, bindingPath, nameID) ?? false;
                }
            }
            else
            {
                return instance?._selectedBindings.BindVisualElementImageToIconBinding(ve, bindingPath, tags) ?? false;
            }
        }

        [SerializeField]
        internal SOButtonIconBindings _selectedBindings;
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