using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UtilEssentials.InputActionBinding
{

    public enum InputBindingCategories
    {
        Keyboard = 0,
        Mouse,
        Gamepad
    }

    [CreateAssetMenu(menuName = "Util Essentials/InputAction Binding/Button Icon Bindings")]
    public class SOButtonIconBindings : ScriptableObject
    {

        [Serializable]
        public class InputLayoutSet
        {
            [SerializeField] string _nameID;
            public string nameID => _nameID;

            [SerializeField] InputBindingCategories _inputBindingCategory;
            public InputBindingCategories inputBindingCategory => _inputBindingCategory;

            [SerializeField] string[] _bindingPaths;
            [SerializeField] Texture2D[] _bindingTextures;
            
            public Texture2D GetTextureFromString(string path)
            {
                for (int i = 0; i < _bindingPaths.Length; i++)
                {
                    if (_bindingPaths[i] == path)
                    {
                        return _bindingTextures[i];
                    }
                }
                return null;
            }
        }

        [SerializeField] List<InputLayoutSet> _inputLayoutSets;

        [SerializeField] int test1;

        public Texture2D GetIcon(string bindingPath, InputBindingCategories category)
        {
            foreach (var set in _inputLayoutSets)
            {
                if (set.inputBindingCategory == category)
                {
                    return set.GetTextureFromString(bindingPath);
                }
            }
            return null;
        }

        public Texture2D GetIcon(string bindingPath, string nameID)
        {
            foreach (var set in _inputLayoutSets)
            {
                if (set.nameID == nameID)
                {
                    return set.GetTextureFromString(bindingPath);
                }
            }
            return null;
        }

        public Texture2D GetIcon(string bindingPath)
        {
            foreach (var set in _inputLayoutSets)
            {
                var tex = set.GetTextureFromString(bindingPath);
                if (tex != null)
                {
                    return tex;
                }
            }
            return null;
        }
    }

//    static class ButtonIconBindingsEditorSettingsMenu
//    {
//        public static SOButtonIconBindings _bindingsSO
//                {
//        get
//        {
//            EditorBuildSettings.TryGetConfigObject(SceneLoaderSettings.CONFIG_NAME, out SceneLoaderSettings settings);
//            return settings;
//        }
//        set
//        {
//            var remove = value == null;
//            if (remove)
//            {
//                EditorBuildSettings.RemoveConfigObject(SceneLoaderSettings.CONFIG_NAME);
//            }
//            else
//            {
//                EditorBuildSettings.AddConfigObject(SceneLoaderSettings.CONFIG_NAME, value, overwrite: true);
//            }
//        }
//    }

//#if UNITY_EDITOR
//        [SettingsProvider]
//        public static SettingsProvider CreateMyCustomSettingsProvider()
//        {
//            // First parameter is the path in the Settings window.
//            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
//            var provider = new SettingsProvider("Project/ButtonIconBindingsEditorSettingsMenu", SettingsScope.Project)
//            {
//                // By default the last token of the path is used as display name if no label is provided.
//                label = "Button Icon Binding",
//                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
//                guiHandler = (searchContext) =>
//                {
//                    EditorGUILayout.ObjectField("Current Settings", _bindingsSO, typeof(SOButtonIconBindings), allowSceneObjects: false);
//                },

//                // Populate the search keywords to enable smart search filtering and label highlighting:
//                keywords = new HashSet<string>(new[] { "Number", "Some String" })
//            };

//            return provider;
//        }
//#endif
//    }

//    public class ButtonIconBindingsEditorSettingsMenu : SettingsProvider
//    {

//        public static SOButtonIconBindings BindingSettings
//        {
//            get { return EditorPrefs.}
//            set { }
//        }

//        public ButtonIconBindingsEditorSettingsMenu(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
//            : base(path, scopes, keywords)
//        { }

//        public override void OnGUI(string searchContext)
//        {
//            base.OnGUI(searchContext);

//            EditorGUILayout.ObjectField
//        }
//    }
}
