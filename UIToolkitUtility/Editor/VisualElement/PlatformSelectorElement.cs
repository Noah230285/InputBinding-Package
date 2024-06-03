using System;
using System.Collections.Generic;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using System.Collections;
using System.Linq;
using UnityEditor.UIElements;

namespace UtilEssentials.UIToolkitUtility.Editor.VisualElements
{
    public class PlatformSelectorElement : VisualElement
    {
        #region UXML Factory

        [Preserve]
        public new class UxmlFactory : UxmlFactory<PlatformSelectorElement, UxmlTraits> { }
        public PlatformSelectorElement()
        {
            Init();
        }
        #endregion

        SerializedProperty _boundProperty;
        EditorWindow _encompassingWindow;
        public EditorWindow encompassingWindow
        {
            get => _encompassingWindow;
            set => _encompassingWindow = value;
        }

        Toggle _includeExcludeToggle;
        Label _includeExcludeTitle;
        VisualElement _optionsContainer;
        Button _selectAllButton;
        Button _deselectAllButton;

        void Init()
        {
            // Load UXML
            string assetPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            string beginingPath = UIToolkitUtilityFunctions.GetBeginningOfPackagePath(assetPath, "com._s.utility_essentials");

            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            $"{beginingPath}/UIToolkitUtility/Assets/Editor/UXML/PlatformSelector.uxml");

            asset.CloneTree(this);

            // Get Elements
            _includeExcludeToggle = this.Q<Toggle>("IncludeExcludeToggle");
            _includeExcludeTitle = this.Q<Label>("IncludeExcludeTitle");
            _optionsContainer = this.Q("OptionsContainer");
            _selectAllButton = this.Q<Button>("SelectAll");
            _deselectAllButton = this.Q<Button>("DeselectAll");


            _includeExcludeToggle.RegisterValueChangedCallback((x) =>
            {
                if ((x.newValue == x.previousValue))
                {
                    return;
                }
                _includeExcludeTitle.text = x.newValue ? "Exclude Platforms" : "Include Platforms";

                var iterator = _boundProperty.Copy();
                iterator.Next(true);
                iterator.Next(true);
                int size = iterator.intValue;
                iterator.Next(true);
                iterator.Next(true);

                for (int i = 1; i < size; i++)
                {
                    iterator.boolValue = !iterator.boolValue;
                    iterator.Next(false);
                }

                _boundProperty.serializedObject.ApplyModifiedProperties();
            });

            _selectAllButton.clicked += () =>
            {
                var iterator = _boundProperty.Copy();
                iterator.Next(true);
                iterator.Next(true);
                int size = iterator.intValue;
                iterator.Next(true);
                iterator.Next(true);

                for (int i = 1; i < size; i++)
                {
                    iterator.boolValue = true;
                    iterator.Next(false);
                }

                _boundProperty.serializedObject.ApplyModifiedProperties();
            };

            _deselectAllButton.clicked += () =>
            {
                var iterator = _boundProperty.Copy();
                iterator.Next(true);
                iterator.Next(true);
                int size = iterator.intValue;
                iterator.Next(true);
                iterator.Next(true);

                for (int i = 1; i < size; i++)
                {
                    iterator.boolValue = false;
                    iterator.Next(false);
                }

                _boundProperty.serializedObject.ApplyModifiedProperties();
            };
        }

        /// <summary>
        /// Binds each element in a bool array to a platform
        /// </summary>
        /// <param name="property">Bool array to be bound</param>
        public void BindToBoolArrayProperty(SerializedProperty property)
        {
            _optionsContainer.Clear();
            _boundProperty = property;

            // String array of all enum names
            var names = Enum.GetNames(typeof(RuntimePlatform));

            List<Toggle> toggles = new();

            _boundProperty.arraySize = names.Length;
            var iterator = _boundProperty.Copy();
            iterator.Next(true);
            iterator.Next(true);
            iterator.Next(true);
            _includeExcludeToggle.BindProperty(iterator);

            iterator.Next(true);

            for (int i = 0; i < names.Length; i++)
            {
                // If the enum at i index is obselete, don't include it
                if (((RuntimePlatform)Enum.Parse(typeof(RuntimePlatform), names[i])).IsObsolete())
                {
                    continue;
                }

                Toggle newPlatformToggle = new();
                newPlatformToggle.label = names[i];

                newPlatformToggle.BindProperty(iterator);

                toggles.Add(newPlatformToggle);

                iterator.Next(false);
            }

            // Sorts the toggles alphabetically and then adds them to the container in that order
            toggles = toggles.OrderBy(x => x.label).ToList();
            foreach (var t in toggles)
            {
                _optionsContainer.Add(t);
            }

            _boundProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}