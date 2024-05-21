using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UtilEssentials.UIToolkitUtility.Editor;
using UtilEssentials.UIToolkitUtility.Editor.VisualElements;

namespace UtilEssentials.UIToolkitUtility.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(SectionAttribute))]
    public class SectionAttributeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SectionAttribute sectionattribute = (attribute as SectionAttribute);
            SectionElement element = new SectionElement(sectionattribute.Name, property);
            SerializedProperty previousProperty = property.FindPreviousProperty();
            foreach (var name in sectionattribute.PropertyNames)
            {
                if (previousProperty == null)
                {
                    element.LinkedAddContent(new PropertyField(property.serializedObject.FindPropertyOrFail(name)));
                }
                else
                {
                    element.LinkedAddContent(new PropertyField(previousProperty.FindPropertyRelativeOrFail(name)));
                }
            }
            return element;
        }
    }
}