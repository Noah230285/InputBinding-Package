using UnityEngine;

namespace UtilEssentials.UIToolkitUtility.Attributes
{
    public class SectionAttribute : PropertyAttribute
    {
        public string Name;
        public string[] PropertyNames;
        public SectionAttribute(string name, string[] propertyName)
        {
            Name = name;
            PropertyNames = propertyName;
        }
    }
}