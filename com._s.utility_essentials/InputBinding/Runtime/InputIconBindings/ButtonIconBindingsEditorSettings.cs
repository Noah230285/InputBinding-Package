using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace UtilEssentials.InputActionBinding
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


    public static Texture2D GetTextureFromBindingPath(string bindingPath, InputBindingCategories category)
        {
            if (instance == null)
            {
                Debug.LogWarning("No 'ButtonIconBindingsEditorSettings' referenced in Project Settings/Button Icon Binding");
                return null;
            }

            if (instance._selectedBindings == null)
            {
                Debug.LogWarning("No 'SOButtonIconBindings' referenced in Project Settings/Button Icon Binding");
                return null;
            }
            Texture2D referencedBindings = instance._selectedBindings.GetIcon(bindingPath, category);
            return referencedBindings;
        }

        public static Texture2D GetTextureFromBindingPath(string bindingPath, string nameID = null)
        {
            if (instance == null)
            {
                Debug.LogWarning("No 'ButtonIconBindingsEditorSettings' referenced in Project Settings/Button Icon Binding");
                return null;
            }

            if (instance._selectedBindings == null)
            {
                Debug.LogWarning("No 'SOButtonIconBindings' referenced in Project Settings/Button Icon Binding");
                return null;
            }
            Texture2D referencedBindings = nameID == null ? instance._selectedBindings.GetIcon(bindingPath) : instance._selectedBindings.GetIcon(bindingPath, nameID);
            return referencedBindings;
        }

        [SerializeField]
        internal SOButtonIconBindings _selectedBindings;

        [SerializeField]
        int test1;

        [SerializeField]
        string test2;
    }

#if UNITY_EDITOR

    // Register a SettingsProvider using IMGUI for the drawing framework:
    internal static class MyCustomSettingsIMGUIRegister
    {

        public const string k_MyCustomSettingsPath = "/ProjectSettings/ButtonIconBindingSettings.asset";
        public static string path => Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length) + k_MyCustomSettingsPath;


        //internal static ButtonIconBindingsEditorSettings GetSOFromConfig()
        //{
            //ButtonIconBindingsEditorSettings settings;
            //do
            //{
            //    if (!File.Exists(path))
            //    {
            //        break;
            //    }
            //    string jsonText = File.ReadAllText(path);

            //    settings = ScriptableObject.CreateInstance<ButtonIconBindingsEditorSettings>();
            //    JsonUtility.FromJsonOverwrite(jsonText, settings);
            //    //settings = JsonUtility.FromJson<ButtonIconBindingsEditorSettings>(jsonText);
            //    if (settings == null)
            //    {
            //        break;
            //    }
            //    return settings;
            //} while (false);

            //settings = CreateNewConfig();
            //return settings;
        //}

        internal static void WriteConfig(ButtonIconBindingsEditorSettings newSettings)
        {
            //if (newSettings != ButtonIconBindingsEditorSettings.staticReference)
            //{
            //    ButtonIconBindingsEditorSettings.staticReference = newSettings._selectedBindings;
            //}
            //var JsonSettings = JsonUtility.ToJson(newSettings);
            //using (StreamWriter sw = new StreamWriter(path))
            //{
            //    sw.Write(JsonSettings);
            //}
            //AssetDatabase.SaveAssets();
        }

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
            //ButtonIconBindingsEditorSettings settings = ScriptableObject.CreateInstance<ButtonIconBindingsEditorSettings>();
            //settings._selectedBindings = null;
            //var JsonSettings = JsonUtility.ToJson(settings);
            //using (StreamWriter sw = new StreamWriter(path))
            //{
            //    sw.Write(JsonSettings);
            //}
            //AssetDatabase.SaveAssets();
            //return settings;
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

                    //var JsonSettings = JsonUtility.ToJson(settings);
                    //using (StreamWriter sw = new StreamWriter(path))
                    //{
                    //    sw.Write(JsonSettings);
                    //}
                    serializedObject.ApplyModifiedProperties();
                    WriteConfig(settings);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Number", "Some String" })
            };

            return provider;
        }
    }
#endif
}