using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UtilEssentials.UIToolkitUtility.Editor;

namespace UtilEssentials.UIToolkitUtility.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(EnumBindingAttribute))]
    public class EnumBindingAttributeDrawer : PropertyDrawer
    {
        int _previousIndex;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new();
            PropertyField enumField = new(property);
            VisualElement bindingContainer = new();
            bindingContainer.name = "Container";
            root.Add(enumField);
            root.Add(bindingContainer);
            SerializedProperty previousProperty = property.FindPreviousProperty();
            for (int i = 0; i < property.enumNames.Length; i++)
            {
                string name = (attribute as EnumBindingAttribute).PropertyNames[i];
                if (previousProperty == null)
                {
                    bindingContainer.Add(new PropertyField(property.serializedObject.FindPropertyOrFail(name)).SetDisplay(false));
                }
                else
                {
                    bindingContainer.Add(new PropertyField(previousProperty.FindPropertyRelativeOrFail(name)).SetDisplay(false));
                }
            }
            enumField.RegisterValueChangeCallback((x) => this.UpdateDisplay(property, bindingContainer));
            UpdateDisplay(property, bindingContainer);
            return root;
        }

        void UpdateDisplay(SerializedProperty property, VisualElement bindingContainer)
        {
            bindingContainer.ElementAt(_previousIndex).SetDisplay(false);
            bindingContainer.ElementAt(property.enumValueIndex).SetDisplay(true);
            _previousIndex = property.enumValueIndex;
        }
    }
}