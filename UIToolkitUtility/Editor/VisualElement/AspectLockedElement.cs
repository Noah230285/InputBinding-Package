using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UtilEssentials.UIToolkitUtility.Editor.VisualElements
{
    public class AspectLockedElement : VisualElement
    {
        [Preserve]
        public new class UxmlFactory : UxmlFactory<AspectLockedElement, UxmlTraits> { }
        public AspectLockedElement()
        {
            this.style.height = 0;
            this.RegisterCallback<GeometryChangedEvent>((x) => Update());
        }

        void Update()
        {
            if (aspectRatio <= 0)
            {
                Debug.LogWarning("Aspect ratio on AspectLockedElement cannot be 0 or negative value");
                return;
            }
            this.style.height = new StyleLength(new Length(contentRect.width / aspectRatio, LengthUnit.Pixel));
        }


        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public UxmlFloatAttributeDescription inputNameAttr = new UxmlFloatAttributeDescription { name = "aspect-ratio", defaultValue = 1 };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                var ate = ve as AspectLockedElement;

                base.Init(ve, bag, cc);
                ate.aspectRatio = inputNameAttr.GetValueFromBag(bag, cc);
            }
        }

        float _aspectRatio;
        public float aspectRatio
        {
            get => _aspectRatio;
            set
            {
                _aspectRatio = value;
                Update();
            }
        }
    }
}