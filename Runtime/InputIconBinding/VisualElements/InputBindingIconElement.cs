using System;
using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UtilEssentials.InputIconBinding.VisualElements
{
    public class InputBindingIconElement : VisualElement
    {
        #region UXML Factory

        [Preserve]
        public new class UxmlFactory : UxmlFactory<InputBindingIconElement, UxmlTraits> { }
        public InputBindingIconElement()
        {
            Init();
        }
        #endregion

        Label _defaultTextLabel;
        Action _cancelAnimationDelegate;

        void Init()
        {
            _defaultTextLabel = new Label();
            _defaultTextLabel.text = _defaultText;
            //_defaultTextLabel.style.position = Position.Absolute;
            Add(_defaultTextLabel);
            UpdateIcon();
        }

        public void UpdateIcon()
        {
            _cancelAnimationDelegate?.Invoke();
            _cancelAnimationDelegate = null;

            if (_inputBindingPath == null || _inputBindingPath == string.Empty)
            {
                style.backgroundImage = null;
                _defaultTextLabel.style.display = DisplayStyle.Flex;
                return;
            }

            if (ButtonIconBindingsEditorSettings.BindVisualElementImageToIconBinding(this, out _cancelAnimationDelegate, _inputBindingPath, _nameid, bindingCategory, _tags, searchType))
            {
                _defaultTextLabel.style.display = DisplayStyle.None;
            }
            else
            {
                _defaultTextLabel.style.display = DisplayStyle.Flex;
            }

        }

        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public UxmlStringAttributeDescription defaultTextAttr = new UxmlStringAttributeDescription { name = "default-text", defaultValue = "Unknown Icon" };
            public UxmlStringAttributeDescription inputBindPathAttr = new UxmlStringAttributeDescription { name = "input-binding-path", defaultValue = string.Empty };

            public UxmlStringAttributeDescription nameIDAttr = new UxmlStringAttributeDescription { name = "nameid", defaultValue = string.Empty };
            public UxmlEnumAttributeDescription<InputBindingCategories> bindingCategoryAttr = new UxmlEnumAttributeDescription<InputBindingCategories> { name = "input-binding-category", defaultValue = 0 };
            public UxmlStringAttributeDescription tagsAttr = new UxmlStringAttributeDescription { name = "tags", defaultValue = "ExampleTag1|ExampleTag2" };

            public UxmlEnumAttributeDescription<InputBindingSearchType> searchTypeAttr = new UxmlEnumAttributeDescription<InputBindingSearchType> { name = "icon-search-type", defaultValue = 0 };


            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                var ate = ve as InputBindingIconElement;
                base.Init(ve, bag, cc);

                ate.defaultText = defaultTextAttr.GetValueFromBag(bag, cc);
                ate.inputBindingPath = inputBindPathAttr.GetValueFromBag(bag, cc);
                ate.nameid = nameIDAttr.GetValueFromBag(bag, cc);
                ate.bindingCategory = bindingCategoryAttr.GetValueFromBag(bag, cc);
                ate.tags = tagsAttr.GetValueFromBag(bag, cc);
                ate.searchType = searchTypeAttr.GetValueFromBag(bag, cc);
                ate.UpdateIcon();
            }
        }

        string _defaultText;
        public string defaultText
        {
            get => _defaultText;
            set
            {
                _defaultText = value;
                _defaultTextLabel.text = _defaultText;
            }
        }

        string _inputBindingPath;
        public string inputBindingPath
        {
            get => _inputBindingPath;
            set
            {
                if (_inputBindingPath != value)
                {
                    _inputBindingPath = value;
                    UpdateIcon();
                }
            }
        }

        string _nameid;
        public string nameid
        {
            get => _nameid;
            set
            {
                if (_nameid != value)
                {
                    _nameid = value;
                }
            }
        }

        InputBindingCategories _bindingCategory;
        public InputBindingCategories bindingCategory
        {
            get => _bindingCategory;
            set
            {
                if (_bindingCategory != value)
                {
                    _bindingCategory = value;
                }
            }
        }

        string[] _tags;
        public string tags
        {
            get
            {
                string combinedString = string.Empty;
                for (int i = 0; i < _tags.Length; i++)
                {
                    combinedString.Insert(combinedString.Length, _tags[i]);
                }
                return combinedString;
            }
            set
            {
                string[] array = value.Split(char.Parse("|"));
                if (_tags != array)
                {
                    _tags = array;
                }
            }
        }

        InputBindingSearchType _searchType;
        public InputBindingSearchType searchType
        {
            get => _searchType;
            set
            {
                if (_searchType != value)
                {
                    _searchType = value;
                }
            }
        }
    }

}
