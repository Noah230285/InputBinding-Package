using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace UtilEssentials.UIToolkitUtility.Editor.VisualElements
{
    public class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, TwoPaneSplitView.UxmlTraits> { }
    }
}