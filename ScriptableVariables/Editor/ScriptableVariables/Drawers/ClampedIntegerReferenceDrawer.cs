using UnityEditor;
using UnityEngine.UIElements;

namespace UtilEssentials.ScriptableVariables.Editor
{
    [CustomPropertyDrawer(typeof(ClampedIntegerReference))]
    class ClampedIntegerReferenceDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            ClampedIntegerElement element = new(property);
            return element;
        }
    }
}