using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UtilEssentials.UIDocumentExtenderer.Editor
{
    public class SliderBindingsDrawer : MonoBehaviour
    {
        [CustomPropertyDrawer(typeof(SliderBindings.AudioMixerValues))]
        public class AudioMixerValuesDrawer : PropertyDrawer
        {
            public override VisualElement CreatePropertyGUI(SerializedProperty property)
            {
                VisualElement root = new();
                root.Add(new PropertyField(property.FindPropertyRelative("Mixer")));
                root.Add(new PropertyField(property.FindPropertyRelative("ValueName")));
                return root;
            }
        }
    }

}
