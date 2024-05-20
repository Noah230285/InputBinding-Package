using UnityEngine;

namespace UtilEssentials.UIToolkitUtility.Attributes
{
    public class EnumBindingAttribute : PropertyAttribute
    {
        public string[] PropertyNames;
        public EnumBindingAttribute(string[] propertyNames)
        {
            PropertyNames = propertyNames;
        }
    }
}