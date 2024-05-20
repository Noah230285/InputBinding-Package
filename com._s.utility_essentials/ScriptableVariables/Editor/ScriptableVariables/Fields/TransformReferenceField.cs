using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UtilEssentials.ScriptableVariables.Editor
{
    internal class TransformReferenceField : ScriptableVariableField<Object>
    {
        #region
        public new class UxmlFactory : UxmlFactory<TransformReferenceField> { }
        public TransformReferenceField() : base(null, "")
        {
        }
        #endregion

        public TransformReferenceField(SerializedProperty variableReferenceProperty, string label = "") : base(variableReferenceProperty, label)
        {
        }

        protected override BaseField<Object> CreateField()
        {
            ObjectField field = new ObjectField(label);
            field.objectType = typeof(Transform);
            return field;
        }
    }
}