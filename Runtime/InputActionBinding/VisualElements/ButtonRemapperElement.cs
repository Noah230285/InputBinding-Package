using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.Events;
using UtilEssentials.UIToolkitUtility.VisualElements;

namespace UtilEssentials.InputActionBinding.UIToolkit
{
    public class ButtonRemapperElement : VisualElement, IRuntimeElement
    {
        #region UXML Factory

        [Preserve]
        public new class UxmlFactory : UxmlFactory<ButtonRemapperElement, UxmlTraits> { }
        public ButtonRemapperElement()
        {
            Init();
        }
        #endregion

        UnityAction _loadFinished;
        bool _loaded;

        public UnityAction loadFinished { get => _loadFinished; set => _loadFinished = value; }
        public bool loaded { get => _loaded; set => _loaded = value; }

        public void Init()
        {
            // Load the source UXML file
            (this as IRuntimeElement).LoadAddressableAsset("InputActionBinding/InputRemapper.uxml");
        }

        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public UxmlStringAttributeDescription inputNameAttr = new UxmlStringAttributeDescription { name = "input-name", defaultValue = "Placeholder Name" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                var ate = ve as ButtonRemapperElement;

                UnityAction func = () =>
                {
                    base.Init(ve, bag, cc);
                    ate.inputName = inputNameAttr.GetValueFromBag(bag, cc);
                };
                if (ate.loaded)
                {
                    func();
                }
                else
                {
                    ate.loadFinished += func;
                }
            }
        }

        public string inputName 
        {
            get { return loaded ? (this.ElementAt(0).ElementAt(0) as Label).text : ""; }
            set { if (!loaded) return; (this.ElementAt(0).ElementAt(0) as Label).text = value; this.name = value; }
        }
    }
}