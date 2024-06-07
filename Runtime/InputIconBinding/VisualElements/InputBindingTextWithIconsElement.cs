using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UtilEssentials.InputIconBinding.VisualElements
{
    public class InputBindingTextWithIconsElement : VisualElement
    {
        #region UXML Factory

        [Preserve]
        public new class UxmlFactory : UxmlFactory<InputBindingTextWithIconsElement, UxmlTraits> { }
        public InputBindingTextWithIconsElement()
        {
            RegisterCallback<GeometryChangedEvent>(Init);
        }
        #endregion

        void Init(GeometryChangedEvent evt)
        {
            UnregisterCallback<GeometryChangedEvent>(Init);
            TextUpdated();
        }

        void TextUpdated()
        {
            if (_totalText == null)
            {
                return;
            }
            List<string> _textSeperations = new() { _totalText};

            List<InputBindingIconElement> iconElements = new();

            for (int i = 0; i < childCount; i++)
            {
                var iconElement = ElementAt(i) as InputBindingIconElement;
                if (iconElement != null)
                {
                    iconElements.Add(iconElement);
                }
            }


            Clear();
            for (int i = 0; i < iconElements.Count; i++)
            {
                string lookForString = $"<icon=\"{i}\">";

                List<string> _dummyTextSeperations = new();
                for (int n = 0; n < _textSeperations.Count; n++)
                {
                    string searchingText = _textSeperations[n];

                    int replaceIndex = searchingText.IndexOf(lookForString);

                    if (replaceIndex < 0)
                    {
                        _dummyTextSeperations.Add(searchingText);
                        continue;
                    }

                    string splitTextOne = searchingText.Substring(0, replaceIndex);

                    int splitTwoStart = replaceIndex + lookForString.Length;
                    string splitTextTwo = searchingText.Substring(splitTwoStart, searchingText.Length - splitTwoStart);

                    _dummyTextSeperations.Add(splitTextOne);
                    _dummyTextSeperations.Add(lookForString);

                    int space = 1;
                    if (splitTextTwo != "")
                    {
                        space++;
                        _dummyTextSeperations.Add(splitTextTwo);
                    }

                    int catchup = 0;
                    foreach (var remainingText in _textSeperations)
                    {
                        if (catchup > n + space)
                        {
                            _dummyTextSeperations.Add(remainingText);
                            continue;
                        }
                        catchup++;
                    }
                }
                _textSeperations = _dummyTextSeperations;
            }

            List<int> remainingIconIndeces = new();
            for (int i = 0; i < iconElements.Count; i++)
            {
                remainingIconIndeces.Add(i);
            }

            foreach (var searchingText in _textSeperations)
            {
                bool foundIcon = false;
                foreach (var index in remainingIconIndeces)
                {
                    if (searchingText == $"<icon=\"{index}\">")
                    {
                        InputBindingIconElement iconElement = iconElements[index];

                        Add(iconElement);
                        iconElement.style.display = DisplayStyle.Flex;
                        remainingIconIndeces.Remove(index);

                        foundIcon = true;
                        break;
                    }
                }
                if (foundIcon)
                {
                    continue;
                }

                Label newLabel = new Label();
                newLabel.text = searchingText;
                Add(newLabel);
            }

            foreach (var index in remainingIconIndeces)
            {
                InputBindingIconElement iconElement = iconElements[index];

                Add(iconElement);
                iconElement.style.display = DisplayStyle.None;
            }
        }



        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public UxmlStringAttributeDescription totalTextAttr = new UxmlStringAttributeDescription { name = "total-text", defaultValue = string.Empty };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                var ate = ve as InputBindingTextWithIconsElement;
                base.Init(ve, bag, cc);

                ate.totalText = totalTextAttr.GetValueFromBag(bag, cc);
            }
        }

        string _totalText;
        public string totalText
        {
            get => _totalText;
            set
            {
                _totalText = value;
                TextUpdated();
            }
        }
    }
}
