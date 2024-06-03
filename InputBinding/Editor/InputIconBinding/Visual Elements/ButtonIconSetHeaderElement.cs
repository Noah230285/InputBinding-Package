using System;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace UtilEssentials.InputIconBinding.Editor
{
    internal class ButtonIconSetHeaderElement : VisualElement
    {
        // Created by thisParent
        ButtonIconBindingsElement _bindingsElement;
        VisualElement _parentContainer;
        VisualElement _bindSetBodiesContainer;

        VisualTreeAsset _buttonIconBinderTreeAsset;

        // Created by this
        Button _bodyButton;
        EnumField _enumField;
        TextField _nameField;
        Button _moveButton;
        VisualElement _icon;
        Button _deleteButton;

        ScrollView _bindSetBody;


        // Adjacent Properties
        SerializedObject _bindingSO;
        SerializedProperty _listProperty;
        SerializedProperty _iconBindsContainersListProperty;

        // Bound Properties
        SerializedProperty _bindingCategoryProperty;
        SerializedProperty _nameIDProperty;
        SerializedProperty _bindingPathsArrayProperty;
        SerializedProperty _bindingDataArrayProperty;
        SerializedProperty _activePlatformsProperty;
        SerializedProperty _tagsProperty;

        // Stops the changing of the binding category to refresh this header
        bool _blockEnumRefresh;

        bool _moving;
        int _moveIndex;
        int _moveUndoGroupIndex;

        int _openIconBindIndex;

        // Action container, sets the size of all of this headers icon binding children when moving the size slider
        Action<float, float> _setAllBindSizes = null;

        Action<ChangeEvent<Enum>> _updateTypeBoundIcon; 
        internal ButtonIconSetHeaderElement(ButtonIconBindingsElement bindingsElement, SerializedProperty iconBindsContainersListProperty, SerializedProperty listProperty)
        {
            // Get properties
            _iconBindsContainersListProperty = iconBindsContainersListProperty.Copy();
            _listProperty = listProperty.Copy();

            _bindingCategoryProperty = _iconBindsContainersListProperty.FindPropertyRelative("_inputBindingCategory");
            _nameIDProperty = _iconBindsContainersListProperty.FindPropertyRelative("_nameID");
            _bindingPathsArrayProperty = _iconBindsContainersListProperty.FindPropertyRelative("_bindingPaths");
            _bindingDataArrayProperty = _iconBindsContainersListProperty.FindPropertyRelative("_bindingData");
            _activePlatformsProperty = _iconBindsContainersListProperty.FindPropertyRelative("_activePlatforms");
            _tagsProperty = _iconBindsContainersListProperty.FindPropertyRelative("_tags");

            // Copy parent element values into this
            _bindingsElement = bindingsElement;
            _parentContainer = _bindingsElement._bindSetHeadersContainer;
            _bindSetBodiesContainer = _bindingsElement._bindSetBodiesContainer;
            _bindingSO = _bindingsElement._activeBindingSOSerializedObject;

            _buttonIconBinderTreeAsset = _bindingsElement._buttonIconBinderTreeAsset;
            
            // Clone from header UXML asset
            _bindingsElement._setHeaderTreeAsset.CloneTree(this);

            // Get header VisualElements
            _bodyButton = this.Q<Button>("BodyButton");
            _enumField = this.Q<EnumField>("EnumField");
            _nameField = this.Q<TextField>("NameField");
            _moveButton = this.Q<Button>("MoveButton");
            _icon = this.Q("Icon");
            _deleteButton = this.Q<Button>("DeleteButton");


            // Moving header
            #region Mouse Events
            EventCallback<MouseMoveEvent> mouseMove = (evt) =>
            {
                float headerHeight = this.contentRect.yMax;
                Vector2 mousePosition = evt.mousePosition;
                Vector2 containerPosition = _parentContainer.worldBound.position;

                int currentIndex = (int)((mousePosition.y - containerPosition.y) / headerHeight);

                currentIndex = Mathf.Clamp(currentIndex, 0, _bindingPathsArrayProperty.arraySize - 1);

                if (_moveIndex != currentIndex)
                {
                    Undo.RegisterFullObjectHierarchyUndo(_bindingSO.targetObject, "Input Binding Header Moved");

                    _moveIndex = currentIndex;
                    _parentContainer.Add(this);
                    for (int i = currentIndex; i < _parentContainer.childCount - 1; i++)
                    {
                        VisualElement child = _parentContainer.ElementAt(currentIndex);
                        _parentContainer.Add(child);
                    }

                    Undo.CollapseUndoOperations(_moveUndoGroupIndex);
                }
            };

            _moveButton.RegisterCallback<MouseDownEvent>((evt) =>
            {
                Undo.SetCurrentGroupName("Full Binding Move");
                _moveUndoGroupIndex = Undo.GetCurrentGroup();

                if (!_moving)
                {
                    Undo.RegisterFullObjectHierarchyUndo(_bindingSO.targetObject, "Start Binding Move");

                    _moving = true;
                    _moveIndex = -1;
                    _moveButton.RegisterCallback(mouseMove, TrickleDown.TrickleDown);
                    this.ElementAt(0).AddToClassList("moveUp");

                    for (int i = 0; i < _parentContainer.childCount; i++)
                    {
                        VisualElement child = _parentContainer.ElementAt(i);
                        child.ElementAt(0).AddToClassList("moving");
                    }

                    Undo.CollapseUndoOperations(_moveUndoGroupIndex);
                }
            }, TrickleDown.TrickleDown);

            _moveButton.RegisterCallback<MouseUpEvent>((evt) =>
            {
                if (_moving)
                {
                    Undo.RegisterFullObjectHierarchyUndo(_bindingSO.targetObject, "End Binding Move");

                    _moving = false;
                    _moveButton.UnregisterCallback(mouseMove, TrickleDown.TrickleDown);
                    this.ElementAt(0).RemoveFromClassList("moveUp");

                    for (int i = 0; i < _parentContainer.childCount; i++)
                    {
                        VisualElement child = _parentContainer.ElementAt(i);
                        child.ElementAt(0).RemoveFromClassList("moving");
                    }

                    if (_moveIndex != -1)
                    {
                        SerializedProperty iterator = listProperty.Copy();
                        iterator.Next(true);
                        iterator.Next(true);
                        int arraySize = iterator.intValue;
                        iterator.Next(true);

                        for (int i = 0; i < arraySize; i++)
                        {
                            if (iterator.propertyPath.Equals(_iconBindsContainersListProperty.propertyPath))
                            {
                                listProperty.MoveArrayElement(i, _moveIndex);
                                break;
                            }
                            iterator.Next(false);
                        }
                    }

                    _bindingSO.ApplyModifiedPropertiesWithoutUndo();
                    Undo.CollapseUndoOperations(_moveUndoGroupIndex);

                    _bindingsElement.Refresh();
                }
            }, TrickleDown.TrickleDown);
            #endregion

            // ScrollView container that contains this header's icon binding children
            _bindSetBody = new ScrollView();
            _bindSetBody.AddToClassList("bindSetBody");
            _bindSetBody.style.display = DisplayStyle.None;
            _bindingsElement._bindSetBodiesContainer.Add(_bindSetBody);

            // Add this to size change broadcast
            _bindingsElement._bindSizeChanged += SetAllIconBindingSizes;

            // Bind header fields
            _enumField.BindProperty(_bindingCategoryProperty);
            _enumField.RegisterValueChangedCallback(BindHeaderMenuEnumIcon);
            _nameField.BindProperty(_nameIDProperty);

            // Bind buttons
            _bodyButton.clicked += OpenHeaderMenu;
            _deleteButton.clicked += DeleteThisBindSet;

            CreateIconBindElements(-1);
        }

        /// <summary>
        /// Opens the header menu and binds it to this header
        /// </summary>
        internal void OpenHeaderMenu()
        {
            _bindingsElement._headerMenuElement.style.display = DisplayStyle.Flex;
            _bindingsElement._iconBindMenuElement.style.display = DisplayStyle.None;

            if (this != _bindingsElement._openMenu)
            {
                _bindingsElement._openMenu?.CloseHeaderMenu();

                this.ElementAt(0).AddToClassList("selected");
                _bindingsElement._openMenu = this;
                _bindSetBody.style.display = DisplayStyle.Flex;


                _bindingsElement._headerMenuTitleLabel.text = _nameIDProperty.stringValue;
                _updateTypeBoundIcon += UpdateHeaderMenuIcon;
                _bindingsElement._headerMenuIconElement.style.backgroundImage = _bindingsElement._bindingPathIcons[_bindingCategoryProperty.enumValueIndex];
                _bindingsElement._headerMenuPlatformSelector.BindToBoolArrayProperty(_activePlatformsProperty);
                _bindingsElement._headerMenuInputCategoryEnumField.BindProperty(_bindingCategoryProperty);
                _bindingsElement._headerMenuTags.BindProperty(_tagsProperty);

                // Delats the binding of the fields to the callbacks so that the initial bindings don't activate them
                _bindingsElement._encompassingWindow.StartCoroutine(WaitToRegisterCallback());
                IEnumerator WaitToRegisterCallback()
                {
                    yield return null;
                    if (_bindingsElement._openMenu != this)
                    {
                        yield break;
                    }
                    _bindingsElement._headerMenuInputCategoryEnumField.RegisterValueChangedCallback(BindHeaderMenuEnumIcon);
                    _bindingsElement._iconBindMenuIconTypeEnumField.RegisterValueChangedCallback(BindIconTypeChanged);
                    _bindingsElement._iconBindMenuTextureField.RegisterValueChangedCallback(IconBindTextureChanged);
                }
            }
        }

        /// <summary>
        /// Updates the header menu's category icon with this header's current category
        /// </summary>
        void UpdateHeaderMenuIcon(ChangeEvent<Enum> evt)
        {
            _bindingsElement._headerMenuIconElement.style.backgroundImage = _bindingsElement._bindingPathIcons[_bindingCategoryProperty.enumValueIndex];
        }

        /// <summary>
        /// Closes the header menu, unbinding it to this
        /// </summary>
        internal void CloseHeaderMenu()
        {
            this.ElementAt(0).RemoveFromClassList("selected");
            _bindSetBody.style.display = DisplayStyle.None;
            _bindingsElement._openMenu = null;

            _updateTypeBoundIcon -= UpdateHeaderMenuIcon;
            _bindingsElement._headerMenuInputCategoryEnumField.UnregisterValueChangedCallback(BindHeaderMenuEnumIcon);

            _bindingsElement._iconBindMenuIconTypeEnumField.UnregisterValueChangedCallback(BindIconTypeChanged);
            _bindingsElement._iconBindMenuTextureField.UnregisterValueChangedCallback(IconBindTextureChanged);
        }

        /// <summary>
        /// Callback event for the header menu enum field
        /// </summary>
        void BindHeaderMenuEnumIcon(ChangeEvent<Enum> evt)
        {
            if (_blockEnumRefresh)
            {
                return;
            }
            CreateIconBindElements((int)(InputBindingCategories)evt.previousValue);
            _updateTypeBoundIcon?.Invoke(evt);
        }

        /// <summary>
        /// Creates an icon bind element for each input binding path defined by the input binding category
        /// </summary>
        /// <param name="previousCategoryIndex">The previous index value of the category enum, is -1 if this is the first time this header element is being created</param>
        void CreateIconBindElements(int previousCategoryIndex)
        {
            _setAllBindSizes = null;
            int bindsIndex = _bindingCategoryProperty.enumValueIndex;
            int bindingsCount = ButtonIconBindingsElement._bindingPathsData[bindsIndex].Length;

            Action clearArray = () =>
            {
                Undo.RegisterCompleteObjectUndo(_bindingSO.targetObject, "Enum category changed");

                _bindingPathsArrayProperty.ClearArray();
                _bindingDataArrayProperty.ClearArray();

                _bindingPathsArrayProperty.arraySize = bindingsCount;
                _bindingDataArrayProperty.arraySize = bindingsCount;

                _bindingSO.ApplyModifiedPropertiesWithoutUndo();
            };

            if ((previousCategoryIndex > 0 && bindsIndex != previousCategoryIndex) || _bindingPathsArrayProperty.arraySize != bindingsCount)
            {
                clearArray();
            }
            _icon.style.backgroundImage = _bindingsElement._bindingPathIcons[bindsIndex];

            SerializedProperty bindingPathsIterator = _bindingPathsArrayProperty.Copy();
            SerializedProperty bindingDataIterator = _bindingDataArrayProperty.Copy();

            bindingPathsIterator.Next(true);
            bindingPathsIterator.Next(true);
            bindingPathsIterator.Next(false);
            bindingDataIterator.Next(true);
            bindingDataIterator.Next(true);
            bindingDataIterator.Next(false);

            _bindSetBody.Clear();


            // Create new Icon Bind
            for (int i = 0; i < bindingsCount; i++)
            {
                VisualElement inputBindElement = new();

                _buttonIconBinderTreeAsset.CloneTree(inputBindElement);
                ObjectField textureField = inputBindElement.Q<ObjectField>("TextureField");
                Label pathLabel = inputBindElement.Q<Label>("Label");
                Button selectionButton = inputBindElement.Q<Button>("SelectionButton");

                inputBindElement.style.flexGrow = 0;
                Action<float, float> setSize = (scale, font) =>
                {
                    inputBindElement.ElementAt(0).style.width = scale;
                    inputBindElement.ElementAt(0).style.height = scale;

                    pathLabel.style.fontSize = font;
                };
                _setAllBindSizes += setSize;

                string bindingPath = ButtonIconBindingsElement._bindingPathsData[bindsIndex][i].Key;
                string displayName = ButtonIconBindingsElement._bindingPathsData[bindsIndex][i].Value;

                string titleText = displayName ?? bindingPath;
                pathLabel.text = titleText;
                textureField.tooltip = bindingPath;

                bindingPathsIterator.stringValue = bindingPath;

                SerializedProperty textureProperty = bindingDataIterator.FindPropertyRelative("_texture");
                textureField.value = textureProperty.objectReferenceValue;
                textureField.BindProperty(textureProperty);

                _bindSetBody.Add(inputBindElement);

                int thisIndex = i;
                selectionButton.clicked += () => OpenIconBindMenu(thisIndex, titleText);

                bindingPathsIterator.Next(false);
                bindingDataIterator.Next(false);
            }

            _bindingSO.ApplyModifiedPropertiesWithoutUndo();
        }

        void OpenIconBindMenu(int index, string titleText)
        {
            _openIconBindIndex = index;

            _bindingsElement._headerMenuElement.style.display = DisplayStyle.None;
            _bindingsElement._iconBindMenuElement.style.display = DisplayStyle.Flex;

            SerializedProperty bindingPathProperty = _bindingPathsArrayProperty.GetArrayElementAtIndex(index);
            SerializedProperty dataProperty = _bindingDataArrayProperty.GetArrayElementAtIndex(index);
            SerializedProperty typeProperty = dataProperty.FindPropertyRelative("_type");
            SerializedProperty textureProperty = dataProperty.FindPropertyRelative("_texture");
            SerializedProperty animationTimeProperty = dataProperty.FindPropertyRelative("_animationTime");


            _bindingsElement._iconBindMenuTitleLabel.text = titleText;
            _bindingsElement._iconBindMenuBindingPathTextField.value = bindingPathProperty.stringValue;

            _bindingsElement._iconBindMenuIconTypeEnumField.value = (IconType)typeProperty.enumValueIndex;
            _bindingsElement._iconBindMenuIconTypeEnumField.BindProperty(typeProperty);
            _bindingsElement._iconBindMenuTextureField.objectType = typeof(Texture2D);
            _bindingsElement._iconBindMenuTextureField.value = textureProperty.objectReferenceValue;
            _bindingsElement._iconBindMenuTextureField.BindProperty(textureProperty);
            _bindingsElement._iconBindMenuAnimationTimeFloatField.BindProperty(animationTimeProperty);

            ChangeIconBindMenuFromTypeEnum((IconType)typeProperty.enumValueIndex);
        }

        void BindIconTypeChanged(ChangeEvent<Enum> evt)
        {
            if (evt.newValue != null)
            {
                ChangeIconBindMenuFromTypeEnum((IconType)evt.newValue);
            }
            else
            {
                ChangeIconBindMenuFromTypeEnum((IconType)evt.previousValue);
            }
        }

        void ChangeIconBindMenuFromTypeEnum(IconType value)
        {
            bool anim = false;
            switch (value)
            {
                case IconType.Static:
                    anim = false;
                    break;
                case IconType.Animated_RolloverLoop:
                    anim = true;
                    break;
                case IconType.Animated_ReverseLoop:
                    anim = true;
                    break;
            }

            if (anim)
            {
                _bindingsElement._iconBindMenuAnimationTimeFloatField.style.display = DisplayStyle.Flex;
            }
            else
            {
                _bindingsElement._iconBindMenuAnimationTimeFloatField.style.display = DisplayStyle.None;
            }

            UpdateIconBindMenuIconTexture(_bindingsElement._iconBindMenuTextureField.value as Texture2D);
        }

        void IconBindTextureChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            Texture2D tex = evt.newValue != null ? (evt.newValue as Texture2D) : (evt.previousValue as Texture2D);
            UpdateIconBindMenuIconTexture(evt.newValue as Texture2D);
        }

        void UpdateIconBindMenuIconTexture(Texture2D tex)
        {
            if (tex == null)
            {
                _bindingsElement._iconBindMenuIconElement.style.backgroundImage = null;
                return;
            }

            SerializedProperty dataProperty = _bindingDataArrayProperty.GetArrayElementAtIndex(_openIconBindIndex);
            SerializedProperty typeProperty = dataProperty.FindPropertyRelative("_type");

            string texPath = AssetDatabase.GetAssetPath(tex);
            UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(texPath);
            if (!(sprites == null || sprites.Length == 0))
            {
                SerializedProperty animSpritesProperty = dataProperty.FindPropertyRelative("_animSprites");

                animSpritesProperty.ClearArray();
                animSpritesProperty.arraySize = sprites.Length;
                SerializedProperty iterator = animSpritesProperty.Copy();
                iterator.Next(true);
                iterator.Next(true);
                for (int i = 0; i < sprites.Length; i++)
                {
                    iterator.Next(false);
                    iterator.objectReferenceValue = sprites[i] as Sprite;
                }

                bool anim;
                switch ((IconType)typeProperty.enumValueIndex)
                {
                    case IconType.Static:
                        anim = false;
                        break;
                    case IconType.Animated_RolloverLoop:
                        anim = true;
                        break;
                    case IconType.Animated_ReverseLoop:
                        anim = true;
                        break;
                    default:
                        anim = false;
                        break;
                }

                if (anim)
                {
                    Sprite[] spriteArray = new Sprite[sprites.Length];
                    for (int i = 0; i < spriteArray.Length; i++)
                    {
                        spriteArray[i] = sprites[i] as Sprite;
                    }
                    _bindingsElement.AnimateIconBindMenuIcon(spriteArray, (IconType)typeProperty.enumValueIndex);
                    return;
                }
            }
            _bindingsElement.STOPAnimateIconBindMenuIcon();
            _bindingsElement._iconBindMenuIconElement.style.backgroundImage = tex;
            _bindingSO.ApplyModifiedProperties();
        }

        void SetAllIconBindingSizes(float scale, float font)
        {
            _setAllBindSizes?.Invoke(scale, font);
            if (_bindSetBody.style.display == DisplayStyle.Flex)
            {
                var fakeOldRect = Rect.zero;
                var fakeNewRect = _bindSetBody.layout;

                // Send message to update scroll
                using var evt = GeometryChangedEvent.GetPooled(fakeOldRect, fakeNewRect);
                evt.target = _bindSetBody.contentContainer;
                _bindSetBody.contentContainer.SendEvent(evt);
            }
        }

        void DeleteThisBindSet()
        {
            SerializedProperty iterator = _listProperty.Copy();

            iterator.Next(true);
            iterator.Next(true);
            int arraySize = iterator.intValue;
            iterator.Next(true);

            for (int i = 0; i < arraySize; i++)
            {
                if (iterator.propertyPath.Equals(_iconBindsContainersListProperty.propertyPath))
                {
                    Undo.RegisterFullObjectHierarchyUndo(_bindingSO.targetObject, "Header Set Deleted");
                    if (i > 0)
                    {

                        (_bindingsElement._bindSetHeadersContainer.ElementAt(i - 1) as ButtonIconSetHeaderElement).OpenHeaderMenu();
                    }
                    else if (_bindingsElement._bindSetHeadersContainer.childCount > 1)
                    {
                        (_bindingsElement._bindSetHeadersContainer.ElementAt(1) as ButtonIconSetHeaderElement).OpenHeaderMenu();
                    }
                    else
                    {
                        CloseHeaderMenu();
                        _bindingsElement._headerMenuElement.style.display = DisplayStyle.None;
                        _bindingsElement._iconBindMenuElement.style.display = DisplayStyle.None;
                    }


                    _listProperty.DeleteArrayElementAtIndex(i);
                    this.parent.Remove(this);

                    _bindingsElement.Refresh();

                    _bindingSO.ApplyModifiedPropertiesWithoutUndo();
                    return;
                }
                iterator.Next(false);
            }
        }
    }
}