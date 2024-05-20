using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UIElements;
using UtilEssentials.UIToolkitUtility;
using UtilEssentials.UIToolkitUtility.Editor;
using static UtilEssentials.InputActionBinding.SOButtonIconBindings;


namespace UtilEssentials.InputActionBinding.Editor
{
    public class ButtonIconBindingsElement : VisualElement
    {
        string[] inputHeaderNames;
        public static readonly string[] _bindingPaths =
        {
        //"None",
        "Keyboard",
        "Mouse",
        "Gamepad"
        };
        readonly static string[] _iconFiles = 
        { 
        "Keyboard.png",
        "Mouse.png",
        "Controller.png"
        };
        Texture2D[] _bindingPathIcons;

        class InputLayoutSetProperties
        {
            public SerializedProperty PrimaryProperty;
            public SerializedProperty BindingCategoryProperty;
            public SerializedProperty BindingPathsArrayProperty;
            public SerializedProperty BindingTexturesArrayProperty;
        }
        List<InputLayoutSetProperties> _inputLayoutSetProperties = new();

        KeyValuePair<string, InputControlLayout.ControlItem>[][] _bindingPathsData;

        VisualElement _bindSetBody;
        VisualElement _bindSetHeadersContainer;
        Button _addSetHeaderButton;
        Slider _bindingSizeSlider;
        Action<float, float> _bindSizeChanged;

        VisualTreeAsset _setHeaderTreeAsset;
        VisualTreeAsset _buttonIconContainerTreeAsset;

        string _beginingPath;

        SerializedObject _activeBindingSOSerializedObject;

        VisualElement _openInputBindContainer;

        bool _blockEnumRefresh;

        public ButtonIconBindingsElement()
        {
            _bindingPathsData = new KeyValuePair<string, InputControlLayout.ControlItem>[_bindingPaths.Length][];
            for (int i = 0; i < _bindingPaths.Length; i++)
            {
                Dictionary<string, InputControlLayout.ControlItem> bindingPaths = new Dictionary<string, InputControlLayout.ControlItem>();
                FindChildBindings(_bindingPaths[i], bindingPaths);
                _bindingPathsData[i] = bindingPaths.ToArray();
            }

            //Find the path for this package
            string assetPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            string beginingPath = UIToolkitUtilityFunctions.GetBeginningOfPackagePath(assetPath, "com._s.utility_essentials");

            _bindingPathIcons = new Texture2D[_iconFiles.Length];
            for (int i = 0; i < _iconFiles.Length; i++)
            {
                _bindingPathIcons[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(
            $"{_beginingPath}/InputBinding/Assets/Editor/InputIconBinding/Textures/{_iconFiles[i]}");
            }

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            $"{_beginingPath}/InputBinding/Assets/Editor/InputIconBinding/UXML/ButtonIconBindingsUXML.uxml");

            visualTree.CloneTree(this);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{_beginingPath}/InputBinding/Assets/Editor/InputIconBinding/USS/ButtonIconBindingsUSS.uss");
            styleSheets.Add(styleSheet);

            this.style.flexGrow = 1;

            _addSetHeaderButton = this.Q("AddInput") as Button;
            _bindSetBody = this.Q("BindSetBody");
            _bindSetHeadersContainer = this.Q("BindSetHeadersContainer");
            _bindingSizeSlider = this.Q("BindingSizeSlider") as Slider;

            _setHeaderTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{_beginingPath}/InputBinding/Assets/Editor/InputIconBinding/UXML/BindSetHeader.uxml");

            _buttonIconContainerTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{_beginingPath}/InputBinding/Assets/Editor/InputIconBinding/UXML/ButtonIconContainer.uxml");

            _addSetHeaderButton.clicked += AddNewLayoutSet;

            const float lowScale = 50;
            const float highScale = 130;

            const float lowFont = 5;
            const float highFont = 13;

            _bindingSizeSlider.value = 0.5f;
            _bindingSizeSlider.RegisterValueChangedCallback((x) =>
            {
                _bindSizeChanged?.Invoke(Mathf.Lerp(lowScale, highScale, x.newValue), Mathf.Lerp(lowFont, highFont, x.newValue));
            });

            Undo.undoRedoPerformed += Refresh;
        }

        ~ButtonIconBindingsElement()
        {
            Undo.undoRedoPerformed -= Refresh;
        }
        void AddNewLayoutSet()
        {
            Undo.RegisterFullObjectHierarchyUndo(_activeBindingSOSerializedObject.targetObject, "Add header");
            // Add a new InputLayoutSet to the list in the scriptable object
            SerializedProperty _inputLayoutSetsProperty = _activeBindingSOSerializedObject.FindProperty("_inputLayoutSets");

            SerializedProperty iterator = _inputLayoutSetsProperty.Copy();

            iterator.Next(true);
            iterator.Next(true);
            int arraySize = iterator.intValue;
            iterator.Next(true);

            _inputLayoutSetsProperty.InsertArrayElementAtIndex(arraySize);

            for (int i = 0; i < arraySize; i++)
            {
                iterator.Next(false);
            }
            GenerateSetFromSerializedProperty(iterator, _inputLayoutSetsProperty);

        }

        void GenerateSetFromSerializedProperty(SerializedProperty inputLayoutSetProperty, SerializedProperty listProperty)
        {
            InputLayoutSetProperties inputLayoutSetProperties = new InputLayoutSetProperties();
            SerializedProperty nameIDProperty = inputLayoutSetProperty.FindPropertyRelative("_nameID");

            inputLayoutSetProperties.PrimaryProperty = inputLayoutSetProperty.Copy();
            inputLayoutSetProperties.BindingCategoryProperty = inputLayoutSetProperty.FindPropertyRelative("_inputBindingCategory");
            inputLayoutSetProperties.BindingPathsArrayProperty = inputLayoutSetProperty.FindPropertyRelative("_bindingPaths");
            inputLayoutSetProperties.BindingTexturesArrayProperty = inputLayoutSetProperty.FindPropertyRelative("_bindingTextures");

            _inputLayoutSetProperties.Add(inputLayoutSetProperties);
            _activeBindingSOSerializedObject.ApplyModifiedProperties();

            var root = new VisualElement();

            _setHeaderTreeAsset.CloneTree(root);

            _bindSetHeadersContainer.Add(root);

            Button bodyButton = root.Q("BodyButton") as Button;
            EnumField enumField = root.Q("EnumField") as EnumField;
            TextField nameField = root.Q("NameField") as TextField;
            VisualElement icon = root.Q("Icon");
            Button deleteButton = root.Q("DeleteButton") as Button;

            ScrollView thisBodyContainer = new ScrollView();
            //thisBodyContainer.direction = SliderDirection.Vertical;
            thisBodyContainer.AddToClassList("bindSetBody");
            _bindSetBody.Add(thisBodyContainer);

            enumField.BindProperty(inputLayoutSetProperties.BindingCategoryProperty);

            Action openMenu = () =>
            {
                if (thisBodyContainer != _openInputBindContainer)
                {
                    thisBodyContainer.style.display = DisplayStyle.Flex;
                    if (_openInputBindContainer != null)
                    {
                        _openInputBindContainer.style.display = DisplayStyle.None;
                    }
                    _openInputBindContainer = thisBodyContainer;

                }
            };
            bodyButton.clicked += openMenu;
            openMenu();

            nameField.BindProperty(nameIDProperty);

            Action<float, float> setAllBindSizes = null;
            Action<float, float> onSetAllBindSizes = (scale, font) =>
            {
                setAllBindSizes?.Invoke(scale, font);
                //thisBodyContainer.style.width = 0;
                //thisBodyContainer.style.width = new StyleLength(StyleKeyword.Auto);

                if (thisBodyContainer.style.display == DisplayStyle.Flex)
                {
                    var fakeOldRect = Rect.zero;
                    var fakeNewRect = thisBodyContainer.layout;

                    using var evt = GeometryChangedEvent.GetPooled(fakeOldRect, fakeNewRect);
                    evt.target = thisBodyContainer.contentContainer;
                    thisBodyContainer.contentContainer.SendEvent(evt);
                }
            };
            _bindSizeChanged += onSetAllBindSizes;
            Action<int> createInputBindings = (int previousValue) =>
            {


                setAllBindSizes = null;
                int bindsIndex = inputLayoutSetProperties.BindingCategoryProperty.enumValueIndex;
                int bindingsCount = _bindingPathsData[bindsIndex].Length;

                Action clearArray = () =>
                {
                    Undo.RegisterCompleteObjectUndo(_activeBindingSOSerializedObject.targetObject, "Enum category changed");

                    inputLayoutSetProperties.BindingPathsArrayProperty.ClearArray();
                    inputLayoutSetProperties.BindingTexturesArrayProperty.ClearArray();

                    inputLayoutSetProperties.BindingPathsArrayProperty.arraySize = bindingsCount;
                    inputLayoutSetProperties.BindingTexturesArrayProperty.arraySize = bindingsCount;

                    _activeBindingSOSerializedObject.ApplyModifiedPropertiesWithoutUndo();
                };

                if (previousValue > 0 && bindsIndex != previousValue)
                {
                    clearArray();
                }
                icon.style.backgroundImage = _bindingPathIcons[bindsIndex];

                SerializedProperty bindingPathsIterator = inputLayoutSetProperties.BindingPathsArrayProperty.Copy();
                SerializedProperty bindingTexturesIterator = inputLayoutSetProperties.BindingTexturesArrayProperty.Copy();



                bindingPathsIterator.Next(true);
                bindingPathsIterator.Next(true);
                bindingPathsIterator.Next(true);
                bindingTexturesIterator.Next(true);
                bindingTexturesIterator.Next(true);
                bindingTexturesIterator.Next(true);

                thisBodyContainer.Clear();

                for (int i = 0; i < bindingsCount; i++)
                {
                    VisualElement inputBindElement = new();

                    _buttonIconContainerTreeAsset.CloneTree(inputBindElement);
                    ObjectField textureField = inputBindElement.Q("TextureField") as ObjectField;
                    Label label = inputBindElement.Q("Label") as Label;

                    inputBindElement.style.flexGrow = 0;
                    Action<float, float> setSize = (scale, font) =>
                    {
                        inputBindElement.ElementAt(0).style.width = scale;
                        inputBindElement.ElementAt(0).style.height = scale;

                        label.style.fontSize = font;
                    };
                    setAllBindSizes += setSize;

                    string bindingPath = _bindingPathsData[bindsIndex][i].Key;

                    string displayName = _bindingPathsData[bindsIndex][i].Value.displayName;
                    label.text = displayName ?? bindingPath;
                    textureField.tooltip = bindingPath;

                    bindingPathsIterator.stringValue = bindingPath;

                    textureField.value = bindingTexturesIterator.objectReferenceValue;
                    textureField.BindProperty(bindingTexturesIterator);

                    thisBodyContainer.Add(inputBindElement);

                    bindingPathsIterator.Next(false);
                    bindingTexturesIterator.Next(false);
                }

                _activeBindingSOSerializedObject.ApplyModifiedPropertiesWithoutUndo();
            };
            enumField.RegisterValueChangedCallback((x) =>
            {
                if (_blockEnumRefresh)
                {
                    return;
                }
                createInputBindings((int)(InputBindingCategories)x.previousValue);
            });
            createInputBindings(-1);

            Action deleteThisBindSet = () =>
            {
                SerializedProperty iterator = listProperty.Copy();

                iterator.Next(true);
                iterator.Next(true);
                int arraySize = iterator.intValue;
                iterator.Next(true);

                for (int i = 0; i < arraySize; i++)
                {
                    if (iterator.propertyPath.Equals(inputLayoutSetProperties.PrimaryProperty.propertyPath))
                    {
                        Undo.RegisterFullObjectHierarchyUndo(_activeBindingSOSerializedObject.targetObject, "Header Set Deleted");

                        listProperty.DeleteArrayElementAtIndex(i);
                        root.parent.Remove(root);
                        _activeBindingSOSerializedObject.ApplyModifiedPropertiesWithoutUndo();
                        return;
                    }
                    iterator.Next(false);
                }
            };

            deleteButton.clicked += deleteThisBindSet;
        }

        public void Refresh()
        {
            _blockEnumRefresh = true;
            CoroutineHost.instance.EndOfFrameAction(() =>
            {
                BindSOButtonIconBindings(_activeBindingSOSerializedObject);
                _blockEnumRefresh = false;
            });
        }

        public void BindSOButtonIconBindings(SerializedObject serializedObject)
        {
            if (serializedObject == null)
            {
                return;
            }

            _bindSetBody.Clear();
            _bindSetHeadersContainer.Clear();

            _activeBindingSOSerializedObject = serializedObject;

            SerializedProperty _inputLayoutSetsProperty = _activeBindingSOSerializedObject.FindProperty("_inputLayoutSets");
            SerializedProperty iterator = _inputLayoutSetsProperty.Copy();

            iterator.Next(true);
            iterator.Next(true);
            int arraySize = iterator.intValue;
            iterator.Next(true);

            for (int i = 0; i < arraySize; i++)
            {
                GenerateSetFromSerializedProperty(iterator, _inputLayoutSetsProperty);
                iterator.Next(false);
            }
        }

        static bool FindChildBindings(string name, Dictionary<string, InputControlLayout.ControlItem> bindingPaths, Stack<string> pathStack = null)
        {
            if (name == null || name == "")
            {
                return false;
            }
            var layout = InputSystem.LoadLayout(name);

            if (layout == null)
            {
                return false;
            }

            if (pathStack == null)
            {
                pathStack = new Stack<string>();
                if (layout.isGenericTypeOfDevice)
                    pathStack.Push($"<{name}>");
                else
                    pathStack.Push(name);
            }

            //has no children, is a binding path
            if (layout.controls.Count == 0)
                return false;

            foreach (var c in layout.controls)
            {
                pathStack.Push(c.name);

                if (!FindChildBindings(c.layout, bindingPaths, pathStack))
                {
                    string bindingPath = string.Join("/", pathStack.Reverse());
                    if (!bindingPaths.ContainsKey(bindingPath))
                    {
                        bindingPaths.Add(bindingPath, c);
                    }
                    pathStack.Pop();
                }
            }

            pathStack.Pop();

            return true;
        }
    }

}
