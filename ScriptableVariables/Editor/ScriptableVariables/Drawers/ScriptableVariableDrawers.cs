using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UtilEssentials.ScriptableVariables.Editor
{
    public class VariableReferenceDrawer<T> : PropertyDrawer
    {
    }

    [CustomPropertyDrawer(typeof(BoolReference))]
    public class BoolReferenceDrawer : VariableReferenceDrawer<bool>
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            BoolReferenceField element = new(property, property.displayName);
            return element;
        }
    }
    [CustomPropertyDrawer(typeof(FloatReference))]
    public class FloatReferenceDrawer : VariableReferenceDrawer<float>
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            FloatReferenceField element = new(property, property.displayName);
            return element;
        }
    }
    [CustomPropertyDrawer(typeof(IntegerReference))]
    public class IntegerReferenceDrawer : VariableReferenceDrawer<int>
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            IntegerReferenceField element = new(property, property.displayName);
            return element;
        }
    }
    [CustomPropertyDrawer(typeof(TransformReference))]
    public class TransformReferenceDrawer : VariableReferenceDrawer<Transform>
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            TransformReferenceField element = new(property, property.displayName);
            return element;
        }
    }
}