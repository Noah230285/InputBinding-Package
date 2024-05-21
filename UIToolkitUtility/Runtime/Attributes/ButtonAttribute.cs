using UnityEngine;

namespace UtilEssentials.UIToolkitUtility.Attributes
{
    public class ButtonAttribute : PropertyAttribute
    {
        public string name { get; private set; }
        public ButtonAttribute(string name)
        {
            this.name = name;
        }
    }
}