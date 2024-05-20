using UtilEssentials.UIToolkitUtility.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UtilEssentials.ScriptableVariables.Editor
{
    [CustomEditor(typeof(ScriptableVariable), true)]
    public class ScriptableVariableEditor : UIToolkitEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = base.CreateInspectorGUI();
            TextField descriptionField = new();
            descriptionField.multiline = true;
            descriptionField.AddToClassList("descriptionBox");
            descriptionField.BindProperty(serializedObject.FindProperty("_description"));
            root.RemoveAt(1);
            root.LinkedAdd(descriptionField, 1);
            return root;
        }
    }
}