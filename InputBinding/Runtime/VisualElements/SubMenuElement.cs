using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using UtilEssentials.UIToolkitUtility.VisualElements;

namespace UtilEssentials.InputActionBinding.UIToolkit
{
    public class SubMenuElement : VisualElement, IRuntimeElement
    {
        #region UXML Factory
        [Preserve]
        public new class UxmlFactory : UxmlFactory<SubMenuElement, UxmlTraits> { }
        public SubMenuElement()
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
            //Load the source UXML file
           (this as IRuntimeElement).LoadAssets("/InputBinding/Assets/InputActionBinding/UIToolkit/VisualTreeAssets/SubMenu.uxml", "com._s.utility_essentials");
        }

        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public UxmlStringAttributeDescription titleLabelAttr = new UxmlStringAttributeDescription { name = "title-label", defaultValue = "Title" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                var ate = ve as SubMenuElement;

                UnityAction func = () =>
                {
                    base.Init(ve, bag, cc);
                    ate.titleLabel = titleLabelAttr.GetValueFromBag(bag, cc);
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

        public string titleLabel
        {
            get { return loaded ? (this.ElementAt(0) as Label).text : ""; }
            set { if (!loaded) return; (this.ElementAt(0) as Label).text = value; }
        }
    }
}