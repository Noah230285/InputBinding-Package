using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UtilEssentials.ScriptableVariables.Editor
{
    internal class FloatReferenceField : ScriptableVariableField<float>
    {
        #region
        public new class UxmlFactory : UxmlFactory<FloatReferenceField> { }
        public FloatReferenceField() : base(null, "FloatEmpty")
        {
            _objectField.objectType = typeof(FloatVariable);
        }
        #endregion

        public FloatReferenceField(SerializedProperty variableReferenceProperty, string label = "FloatEmpty") : base(variableReferenceProperty, label)
        {
            _objectField.objectType = typeof(FloatVariable);
        }

        protected override BaseField<float> CreateField()
        {
            return new FloatField(label);
        }
    }
}