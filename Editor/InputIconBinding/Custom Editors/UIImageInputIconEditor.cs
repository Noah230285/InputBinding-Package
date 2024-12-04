using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UtilEssentials.InputActionBinding.UIToolkit;

namespace UtilEssentials.InputIconBinding.Editor
{
    [CustomEditor(typeof(UIImageInputIcon))]
    public class UIImageInputIconEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var actionProperty = serializedObject.FindProperty("_displayedActionInput");

            var actionField = new PropertyField(actionProperty);

            root.Add(actionField);
            root.Add(new InputActionBinderDrawer.InputBindElement(actionField, actionProperty, serializedObject.FindProperty("_staticReferenceBinding")));
            root.Add(new PropertyField(serializedObject.FindProperty("_iconImage")));
            root.Add(new PropertyField(serializedObject.FindProperty("_iconBackupText")));
            root.Add(new PropertyField(serializedObject.FindProperty("_inputBindingSearchType")));
            root.Add(new PropertyField(serializedObject.FindProperty("_nameID")));
            root.Add(new PropertyField(serializedObject.FindProperty("_tags")));


            return root;
        }
    }
}