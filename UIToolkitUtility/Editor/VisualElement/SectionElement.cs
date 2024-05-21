using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UtilEssentials.UIToolkitUtility.Editor.VisualElements
{
    public class SectionElement : VisualElement
    {
        public Label label;
        public VisualElement arrow;
        public VisualElement content;

        public SectionElement(string name, SerializedProperty extended)
        {
            //Find the path for this package
            string assetPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            string beginingPath = UIToolkitUtilityFunctions.GetBeginningOfPackagePath(assetPath, "com._s.utility_essentials");

            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            $"{beginingPath}/UIToolkitUtility/Assets/Editor/UXML/Section.uxml");

            asset.CloneTree(this);
            //this.AddToClassList("panel");
            //this.ElementAt(0).MergeIntoParent();

            label = this.ElementAt(0).ElementAt(0) as Label;
            arrow = this.ElementAt(0).ElementAt(0).ElementAt(0);
            content = this.ElementAt(0).ElementAt(1);

            label.text = name;

            label.RegisterCallback<ClickEvent>((x) => UIToolkitUtilityFunctions.FlipFlopProperty(extended));
            label.RegisterCallback<ClickEvent>((x) => SetExtended(extended));
            SetExtended(extended);
        }

        public void SetExtended(SerializedProperty extended)
        {
            if (extended.boolValue)
            {
                content.RemoveFromClassList("hidden");
                arrow.AddToClassList("rotate90Anim");
            }
            else
            {
                content.AddToClassList("hidden");
                arrow.RemoveFromClassList("rotate90Anim");
            }
            extended.serializedObject.ApplyModifiedProperties();
        }

        public SectionElement LinkedAddContent(VisualElement element)
        {
            content.LinkedAdd(element);
            return this;
        }

    }
}