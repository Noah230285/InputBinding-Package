using UnityEditor;
using UnityEngine.UIElements;

namespace UtilEssentials.ScriptableVariables.Editor
{
    [CustomPropertyDrawer(typeof(ClampedFloatReference))]
    class ClampedFloatReferenceDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            ClampedFloatElement element = new(property);
            return element;
        }
    }
}