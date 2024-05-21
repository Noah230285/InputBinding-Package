using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Events;

namespace UtilEssentials.UIToolkitUtility.Editor
{

#if !UNITY_2022_1_OR_NEWER
[CustomEditor(typeof(Object), true, isFallback = true)]
#endif
    public class UIToolkitEditor : UnityEditor.Editor
    {
#if !UNITY_2022_1_OR_NEWER
        protected StyleSheet defaultStyles;

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();

            var iterator = serializedObject.GetIterator();

            string assetPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            string beginingPath = UIToolkitUtilityFunctions.GetBeginningOfPackagePath(assetPath, "com._s.utility_essentials");

            defaultStyles = AssetDatabase.LoadAssetAtPath<StyleSheet>(
            $"{beginingPath}/UIToolkit Utility/Editor/USS/UXMLUtility.uss");
            container.styleSheets.Add(defaultStyles);

            if (iterator.NextVisible(true))
            {
                do
                {
                    var propertyField = new PropertyField(iterator.Copy()) { name = "PropertyField:" + iterator.propertyPath };
                    propertyField.Bind(serializedObject);
                    if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
                        propertyField.SetEnabled(value: false);
                    container.Add(propertyField);
                }
                while (iterator.NextVisible(false));
            }
            serializedObject.ApplyModifiedProperties();

            return container;
        }
#endif

    }
}
