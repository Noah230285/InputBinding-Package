using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
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

        void Init()
        {
            UpdateIcon();
        }

        void UpdateIcon()
        {
            ButtonIconBindingsEditorSettings.BindVisualElementImageToIconBinding(this, _inputBindingPath, _bindingGroupName);
        }

        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public UxmlStringAttributeDescription bindingGroupNameAttr = new UxmlStringAttributeDescription { name = "binding-group-name", defaultValue = string.Empty };
            public UxmlStringAttributeDescription bindingPathAttr = new UxmlStringAttributeDescription { name = "input-binding-path", defaultValue = string.Empty};

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                var ate = ve as InputBindingIconElement;
                base.Init(ve, bag, cc);

                ate.bindingGroupName = bindingGroupNameAttr.GetValueFromBag(bag, cc);
                ate.inputBindingPath = bindingPathAttr.GetValueFromBag(bag, cc);
            }
        }

        string _bindingGroupName;
        public string bindingGroupName
        {
            get => _bindingGroupName;
            set
            {
                _bindingGroupName = value;
                UpdateIcon();
            }
        }

        string _inputBindingPath;
        public string inputBindingPath
        {
            get => _inputBindingPath;
            set
            {
                _inputBindingPath = value;
                UpdateIcon();
            }
        }
    }

}
