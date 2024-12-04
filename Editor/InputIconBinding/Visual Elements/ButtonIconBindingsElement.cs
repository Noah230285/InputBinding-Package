using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UtilEssentials.UIToolkitUtility.Editor;
using UtilEssentials.UIToolkitUtility.Editor.VisualElements;


namespace UtilEssentials.InputIconBinding.Editor
{
    public class ButtonIconBindingsElement : VisualElement
    {
        // Strings that align with InputBindingCategories
        internal static readonly string[] _bindingPaths =
        {
        "Keyboard",
        "Mouse",
        "Gamepad"
        };
        internal readonly static string[] _iconFiles = 
        { 
        "Keyboard.png",
        "Mouse.png",
        "Controller.png",
        "Custom.png"
        };
        internal Texture2D[] _bindingPathIcons;

        // Binding path + display name 
        internal static KeyValuePair<string, string>[][] _bindingPathsData;

        // Misc child elements
        internal VisualElement _bindSetBodiesContainer;
        internal Button _customBindAddSet;
        internal VisualElement _bindSetHeadersContainer;
        internal Button _addSetHeaderButton;
        internal Slider _bindingSizeSlider;
        internal VisualElement _overlayElement;

        // Header menu elements
        internal VisualElement _headerMenuElement;
        internal Label _headerMenuTitleLabel;
        internal VisualElement _headerMenuIconElement;
        internal EnumField _headerMenuInputCategoryEnumField;
        internal PlatformSelectorElement _headerMenuPlatformSelector;
        internal IMGUIContainer _headerMenuTags;

        // Icon bind menu elements
        internal VisualElement _iconBindMenuElement;
        internal TextField _iconBindMenuTitleLabel;
        internal TextField _iconBindMenuBindingPathTextField;
        internal EnumField _iconBindMenuIconTypeEnumField;
        internal ObjectField _iconBindMenuTextureField;
        internal Vector2IntField _iconBindMenuFlipBookSizeVec2IntField;
        internal FloatField _iconBindMenuFramesFloatField;
        internal FloatField _iconBindMenuAnimationTimeFloatField;
        internal VisualElement _iconBindMenuIconElement;

        // Visual tree assets
        internal VisualTreeAsset _setHeaderTreeAsset;
        internal VisualTreeAsset _buttonIconBinderTreeAsset;

        // Misc object references
        internal SerializedObject _activeBindingSOSerializedObject;
        internal EditorWindow _encompassingWindow;

        // Values for the icon bind size slider
        const float _lowScale = 50;
        const float _highScale = 130;

        const float _lowFontSize = 5;
        const float _highFontSize = 13;

        // Misc
        internal bool _blockEnumRefresh;
        internal Action _onCloseMenu;
        internal Action<float, float> _bindSizeChanged;
        internal ButtonIconSetHeaderElement _openMenu;


        public ButtonIconBindingsElement(EditorWindow encompassingWindow)
        {
            _encompassingWindow = encompassingWindow;

            // Get the binding paths of each binding category
            if (_bindingPathsData == null)
            {
                _bindingPathsData = new KeyValuePair<string, string>[_bindingPaths.Length][];
                for (int i = 0; i < _bindingPaths.Length; i++)
                {
                    Dictionary<string, string> bindingPaths = new Dictionary<string, string>();
                    FindChildBindings(_bindingPaths[i], "", bindingPaths);
                    _bindingPathsData[i] = bindingPaths.ToArray();
                }
            }

            //Find the path for this package
            string assetPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            string beginningPath = UIToolkitUtilityFunctions.GetBeginningOfPackagePath(assetPath, "com.utility_essentials.input_binding");

            _bindingPathIcons = new Texture2D[_iconFiles.Length];
            for (int i = 0; i < _iconFiles.Length; i++)
            {
                _bindingPathIcons[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(
            $"{beginningPath}/Assets/Editor/InputIconBinding/Textures/{_iconFiles[i]}");
            }
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            $"{beginningPath}/Assets/Editor/InputIconBinding/UXML/ButtonIconBindingsUXML.uxml");

            visualTree.CloneTree(this);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{beginningPath}/Assets/Editor/InputIconBinding/USS/ButtonIconBindingsUSS.uss");
            styleSheets.Add(styleSheet);
            this.style.flexGrow = 1;

            // Get misc child elements
            _addSetHeaderButton = this.Q<Button>("AddInput");
            _bindSetBodiesContainer = this.Q<VisualElement>("BindSetBody");

            _customBindAddSet = this.Q<Button>("custom-add-bind-set");
            _bindSetHeadersContainer = this.Q<VisualElement>("BindSetHeadersContainer");
            _bindingSizeSlider = this.Q<Slider>("BindingSizeSlider");
            _overlayElement = this.Q<VisualElement>("Overlay");

            _overlayElement.style.display = DisplayStyle.Flex;
            _addSetHeaderButton.clicked += AddNewIconBindsContainerHeader;
            
            _customBindAddSet.clicked += AddNewBindSetForCustom;
            _customBindAddSet.style.display = DisplayStyle.None;

            _bindingSizeSlider.value = 0.5f;
            _bindingSizeSlider.RegisterValueChangedCallback((x) =>
            {
                _bindSizeChanged?.Invoke(Mathf.Lerp(_lowScale, _highScale, x.newValue), Mathf.Lerp(_lowFontSize, _highFontSize, x.newValue));
            });


            // Get header menu elements
            _headerMenuElement = this.Q<VisualElement>("HeaderMenu");
            _headerMenuTitleLabel = _headerMenuElement.Q<Label>("HeaderMenuTitle");
            _headerMenuIconElement = _headerMenuElement.Q<VisualElement>("HeaderMenuIcon");
            _headerMenuInputCategoryEnumField = _headerMenuElement.Q<EnumField>("HeaderMenuInputCategory");
            _headerMenuPlatformSelector = _headerMenuElement.Q<PlatformSelectorElement>("HeaderMenuPlatformSelector");
            _headerMenuTags = _headerMenuElement.Q<IMGUIContainer>("HeaderMenuTags");

            _headerMenuPlatformSelector.encompassingWindow = _encompassingWindow;


            // Get icon bind menu elements
            _iconBindMenuElement =  this.Q<VisualElement>("IconBindMenu"); 
            _iconBindMenuTitleLabel = _iconBindMenuElement.Q<TextField>("IconBindMenuTitle");
            _iconBindMenuBindingPathTextField = _iconBindMenuElement.Q<TextField>("IconBindMenuBindingPathLabel");
            _iconBindMenuIconTypeEnumField = _iconBindMenuElement.Q<EnumField>("IconBindMenuIconType");
            _iconBindMenuTextureField = _iconBindMenuElement.Q<ObjectField>("IconBindMenuTextureField");
            _iconBindMenuAnimationTimeFloatField = _iconBindMenuElement.Q<FloatField>("IconBindMenuAnimationTime");
            _iconBindMenuIconElement = _iconBindMenuElement.Q<VisualElement>("IconBindMenuIcon");

            _iconBindMenuAnimationTimeFloatField.RegisterValueChangedCallback((x) =>
            {
                _iconAnimationLoopTime = x.newValue;
            });


            // Load the visual assets that will be copied from several times
            _setHeaderTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{beginningPath}/Assets/Editor/InputIconBinding/UXML/BindSetHeader.uxml");
            _buttonIconBinderTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{beginningPath}/Assets/Editor/InputIconBinding/UXML/ButtonIconContainer.uxml");


             Undo.undoRedoPerformed += Refresh;
        }

        ~ButtonIconBindingsElement()
        {
            Undo.undoRedoPerformed -= Refresh;
        }

        /// <summary>
        /// Add a new IconBindsContainer to the list in the scriptable object, and a VisualElement to access it to this
        /// </summary>
        void AddNewIconBindsContainerHeader()
        {
            Undo.RegisterFullObjectHierarchyUndo(_activeBindingSOSerializedObject.targetObject, "Add icon binds container header");

            // Get the InputLayoutSet list in the scriptable object and append a new element to the end of it
            SerializedProperty _iconBindsContainersListProperty = _activeBindingSOSerializedObject.FindProperty("_iconBindsContainersList");

            SerializedProperty iterator = _iconBindsContainersListProperty.Copy();

            iterator.Next(true);
            iterator.Next(true);
            int arraySize = iterator.intValue;
            _iconBindsContainersListProperty.InsertArrayElementAtIndex(arraySize);
            iterator.Next(true);

            for (int i = 0; i < arraySize; i++)
            {
                iterator.Next(false);
            }

            _activeBindingSOSerializedObject.ApplyModifiedProperties();

            _bindSetHeadersContainer.Add(new ButtonIconSetHeaderElement(this, iterator, _iconBindsContainersListProperty));
        }

        void AddNewBindSetForCustom()
        {
            _openMenu.AddNewCustomIconBind();
        }

        /// <summary>
        /// Wait until the end of the frame to reconstruct the header and body visual elements
        /// </summary>
        public void Refresh()
        {
            _blockEnumRefresh = true;
            _encompassingWindow.StartCoroutine(RefreshWait());
            IEnumerator RefreshWait()
            {
                yield return null;
                BindSOButtonIconBindings(_activeBindingSOSerializedObject);
                _blockEnumRefresh = false;
            }
        }

        /// <summary>
        /// Binds a SOBUttonIconBindings to this element using its serializedObject
        /// </summary>
        /// <param name="SOserializedObject"> The serialized object of the ScriptableObject this is currently bound to</param>
        public void BindSOButtonIconBindings(SerializedObject SOserializedObject)
        {
            if (SOserializedObject == null)
            {
                this.Q<VisualElement>("Overlay").style.display = DisplayStyle.Flex;
                return;
            }

            if (_activeBindingSOSerializedObject == null)
            {
                this.Q<VisualElement>("Overlay").style.display = DisplayStyle.None;
            }


            _bindSetBodiesContainer.Clear();
            _bindSetHeadersContainer.Clear();

            _activeBindingSOSerializedObject = SOserializedObject;

            SerializedProperty _iconBindsContainersListProperty = _activeBindingSOSerializedObject.FindProperty("_iconBindsContainersList");
            SerializedProperty iterator = _iconBindsContainersListProperty.Copy();

            iterator.Next(true);
            iterator.Next(true);
            int arraySize = iterator.intValue;
            iterator.Next(true);

            if (arraySize == 0)
            {
                _headerMenuElement.style.display = DisplayStyle.None;
                _iconBindMenuElement.style.display = DisplayStyle.None;
                return;
            }

            for (int i = 0; i < arraySize; i++)
            {
                _bindSetHeadersContainer.Add(new ButtonIconSetHeaderElement(this, iterator, _iconBindsContainersListProperty));
                iterator.Next(false);
            }
            if (_bindSetHeadersContainer.childCount > 0)
            {
                (_bindSetHeadersContainer.ElementAt(0) as ButtonIconSetHeaderElement).OpenHeaderMenu();
            }
        }

        /// <summary>
        /// Finds all of the bindings for the input category
        /// </summary>
        /// <param name="name">Name of the input</param>
        /// <param name="displayName">Display name of the input</param>
        /// <param name="bindingPaths">Dictionary with binding path as the key and display name as the value</param>
        /// <param name="pathStack">The binding path that the is being searched for child bindings</param>
        /// <returns>Whether there are children of the input binding/the input binding is valid</returns>
        static bool FindChildBindings(string name, string displayName, Dictionary<string, string> bindingPaths, Stack<string> pathStack = null)
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

            bindingPaths.Add(string.Join("/", pathStack.Reverse()), displayName);
            foreach (var c in layout.controls)
            {
                pathStack.Push(c.name);

                if (!FindChildBindings(c.layout, c.displayName, bindingPaths, pathStack))
                {
                    string bindingPath = string.Join("/", pathStack.Reverse());
                    if (!bindingPaths.ContainsKey(bindingPath))
                    {
                        string dN = c.displayName;
                        if (dN == "X" || dN == "Y" || dN == "Up" || dN == "Down" || dN == "Left" || dN == "Right")
                        {
                            dN = $"{displayName} {dN}";
                        }
                        bindingPaths.Add(bindingPath, dN);
                    }
                    pathStack.Pop();
                }
            }

            pathStack.Pop();

            return true;
        }

        EditorCoroutine _iconAnimationCoroutine;
        float _iconAnimationLoopTime;
        public void STOPAnimateIconBindMenuIcon()
        {
            if (_iconAnimationCoroutine != null)
            {
                _encompassingWindow.StopCoroutine(_iconAnimationCoroutine);
            }
            _iconAnimationCoroutine = null;
        }


        public void AnimateIconBindMenuIcon(Sprite[] spriteArray, IconType type)
        {
            if (_iconAnimationCoroutine != null)
            {
                _encompassingWindow.StopCoroutine(_iconAnimationCoroutine);
            }

            _iconAnimationCoroutine = _encompassingWindow.StartCoroutine(IconAnimation(spriteArray, type));
        }
        IEnumerator IconAnimation(Sprite[] spriteArray, IconType type)
        {
            bool reversed = false;
            int length = spriteArray.Length;
            int currentIndex = 0;
            float waitTime = 0;
            double previousTotalTime = EditorApplication.timeSinceStartup;
            _iconBindMenuIconElement.style.backgroundImage = new StyleBackground(spriteArray[0]);
            while (true)
            {
                bool switchTex = false;
                while (_iconAnimationLoopTime / (float)length <= waitTime)
                {
                    switchTex = true;
                    if (_iconAnimationLoopTime <= 0)
                    {
                        waitTime = 0;
                        break;
                    }
                    waitTime -= _iconAnimationLoopTime / (float)length;
                }
                if (switchTex)
                {
                    currentIndex = reversed ? currentIndex - 1 : currentIndex + 1;
                    if (currentIndex == length || currentIndex < 0)
                    {
                        switch (type)
                        {
                            case IconType.Animated_ReverseLoop:
                                reversed = !reversed;
                                currentIndex = reversed ? currentIndex - 1 : currentIndex + 1;
                                break;
                            case IconType.Animated_RolloverLoop:
                                currentIndex = 0;
                                break;
                            default:
                                break;
                        }
                    }
                }
                _iconBindMenuIconElement.style.backgroundImage = new StyleBackground(spriteArray[currentIndex]);

                yield return null;
                waitTime += (float)(EditorApplication.timeSinceStartup - previousTotalTime);
                previousTotalTime = EditorApplication.timeSinceStartup;
            }
        }
    }
}
