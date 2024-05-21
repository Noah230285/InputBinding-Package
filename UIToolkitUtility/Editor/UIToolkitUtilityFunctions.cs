using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UtilEssentials.UIToolkitUtility.Editor
{
    public class SpaceElement : VisualElement
    {
        public SpaceElement(float space)
        {
            style.marginTop = space / 2;
            style.marginBottom = space / 2;
        }
    }



    public static class UIToolkitUtilityFunctions
    {
        ///<Summary>
        ///Inverts the bool value of a Serialized Property.
        ///</Summary>
        ///<returns>
        ///The value of the Serialized Property.
        ///</returns>
        public static bool FlipFlopProperty(this SerializedProperty property)
        {
            property.boolValue = property.boolValue ? false : true;
            property.serializedObject.ApplyModifiedProperties();
            return property.boolValue;
        }

        ///<Summary>
        ///Adds a child element to this element.
        ///</Summary>
        ///<param name="child">
        ///The child element to be added.
        ///</param>
        ///<param name="index">
        ///The index in the parent's child hierachy that the child will be added at.
        ///</param>    
        ///<returns>
        ///The base element.
        ///</returns>
        public static VisualElement LinkedAdd(this VisualElement parent, VisualElement child, int index = -1)
        {
            if (index < 0 || index >= parent.childCount)
            {
                parent.Add(child);
                return parent;
            }
            parent.PushChildrenForward(parent.childCount - index);
            parent.Add(child);
            parent.PushChildrenForward(index + 1);
            return parent;
        }

        ///<Summary>
        ///Adds a class to an element
        ///</Summary>
        ///<param name="className">
        ///The name of the class to be added.
        ///</param>
        ///<returns>
        ///The base element.
        ///</returns>
        public static VisualElement LinkedAddClass(this VisualElement element, string className)
        {
            element.AddToClassList(className);
            return element;
        }

        ///<Summary>
        ///Binds a Serialized property to an Object Field.
        ///</Summary>
        ///<returns>
        ///The base Object Field.
        ///</returns>
        public static ObjectField LinkedBindProperty(this ObjectField element, SerializedProperty property, bool setName = false)
        {
            element.objectType = property.GetUnderlyingType();
            element.BindProperty(property);
            if (setName) { element.label = property.displayName; }
            return element;
        }

        ///<Summary>
        ///Binds a Serialized property to a Field.
        ///</Summary>
        ///<returns>
        ///The base element.
        ///</returns>
        public static VisualElement LinkedBindProperty<T>(this BaseField<T> element, SerializedProperty property, bool setName = false)
        {
            element.BindProperty(property);
            if(property.GetUnderlyingValue() is T)
                element.value = (T)property.GetUnderlyingValue();
            if (setName) { element.label = property.displayName; }
            return element as VisualElement;
        }

        ///<Summary>
        ///Adds a tooltip to a visual element.
        ///</Summary>
        ///<returns>
        ///The base element.
        ///</returns>
        public static VisualElement LinkedTooltip(this VisualElement element, string text)
        {
            element.tooltip = text;
            return element;
        }

        ///<Summary>
        ///Adds a style sheet to a visual element.
        ///</Summary>
        ///<returns>
        ///The base element.
        ///</returns>
        public static VisualElement LinkedAddStyleSheet(this VisualElement element, StyleSheet sheet)
        {
            element.styleSheets.Add(sheet);
            return element;
        }

        ///<Summary>
        ///Makes a field read-only.
        ///</Summary>
        ///<returns>
        ///The base field.
        ///</returns>
        public static BaseField<T> LinkedReadOnly<T>(this BaseField<T> element)
        {
            switch (typeof(T).Name)
            {
                //case "bool":
                //    element = new ScriptableVariableField<bool>(property, property.displayName);
                //    break;
                case "Single":
                    (element as BaseField<float>).LinkedReadOnly();
                    break;
                //case "int":
                //    element = new ScriptableVariableField<int>(property, property.displayName); 
                //    break;
                default:
                    element = null;
                    Debug.LogWarning($"Trying to make unsupported type {typeof(T)} into read-only");
                    break;
            }
            element.AddToClassList("readOnly");
            return element;
        }

        ///<Summary>
        ///Makes a field read-only.
        ///</Summary>
        ///<returns>
        ///The base field.
        ///</returns>
        public static BaseField<float> LinkedReadOnly(this BaseField<float> element)
        {
            (element as FloatField).isReadOnly = true;
            element.AddToClassList("readOnly");
            return element;
        }

        ///<Summary>
        ///Makes a toggle instantly undo any change that is made on click.
        ///</Summary>
        ///<returns>
        ///The base element.
        ///</returns>
        public static BaseField<bool> LinkedReadOnly(this BaseField<bool> element)
        {
            element.RegisterCallback<ClickEvent>((x) => Undo.PerformUndo());
            element.AddToClassList("readOnly");
            return element;
        }

        ///<Summary>
        ///Makes a Vector3 Field read-only.
        ///</Summary>
        ///<returns>
        ///The base element.
        ///</returns>
        public static BaseField<Vector3> LinkedReadOnly(this BaseField<Vector3> element)
        {
            (element.ElementAt(0).ElementAt(0) as FloatField).isReadOnly = true;
            (element.ElementAt(0).ElementAt(1) as FloatField).isReadOnly = true;
            (element.ElementAt(0).ElementAt(2) as FloatField).isReadOnly = true;
            element.AddToClassList("readOnly");
            return element;
        }

        ///<Summary>
        ///Makes a Text Field read-only.
        ///</Summary>
        ///<returns>
        ///The base element.
        ///</returns>
        public static TextInputBaseField<T> LinkedReadOnly<T>(this TextInputBaseField<T> element)
        {
            element.isReadOnly = true;
            element.AddToClassList("readOnly");
            return element;
        }

        public static VisualElement LinkedSetName(this VisualElement element, string name)
        {
            element.name = name;
            return element;
        }

        ///<Summary>
        ///Adds a Change Callback on this Object Field that redraws a Property Drawer.
        ///</Summary>
        ///<param name="drawerAction">
        ///The draw function that will be called to fill the root element whenever the field is changed.
        ///</param>
        ///<param name="root">
        ///The root element that contains the property drawer's content.
        ///</param>
        ///<param name="property">
        ///Serialized property of the property drawer. 
        ///</param>
        ///<returns>
        ///The base Object Field;
        ///</returns>
        public static ObjectField ReloadOnChange(this ObjectField field, Action<VisualElement, SerializedObject> drawerAction, VisualElement root, SerializedProperty property)
        {
            field.RegisterValueChangedCallback((x) =>
            {
                if (x.newValue == x.previousValue) { return; }
                root.Clear();
                if (x.newValue != null)
                {
                    drawerAction(root, new SerializedObject(EditorHelper.GetTargetObjectOfProperty(property) as UnityEngine.Object));
                }
            });
            return field;
        }

        public static VisualElement BindElementDisplay(this Toggle field, VisualElement element, bool inverted = false)
        {
            element.SetDisplay(field.value);
            field.RegisterValueChangedCallback((x) =>
            {
                if (x.newValue == x.previousValue) { return; }
                element.SetDisplay(x.newValue != inverted);
            });
            return field;
        }

        public static VisualElement BindElementDisplay(this Button field, SerializedProperty property, VisualElement element, bool inverted = false)
        {
            element.SetDisplay(property.boolValue);
            field.clicked += () =>
            {
                property.FlipFlopProperty();
                element.SetDisplay(property.boolValue != inverted);
            };
            return field;
        }

        public static VisualElement LinkedSetWidthPercent(this VisualElement element, float percent)
        {
            element.style.width = Length.Percent(percent);
            return element;
        }

        public static VisualElement SetDisplay(this VisualElement element, bool isVisible)
        {
            if (isVisible)
            {
                element.RemoveFromClassList("hidden");
            }
            else
            {
                element.AddToClassList("hidden");
            }
            return element;
        }

        /////<Summary>
        /////Merges this element into it's parent.
        /////</Summary>
        /////<returns>
        /////The base element's parent.
        /////</returns>
        //public static VisualElement MergeIntoParent(this VisualElement element)
        //{
        //    VisualElement parent = element.parent;
        //    for (int i = element.childCount - 1; i >= 0; i--)
        //    {
        //        parent.Add(element.ElementAt(0));
        //    }
        //    parent.Remove(element);
        //    return parent;
        //}

        ///<Summary>
        ///Push each of this element's children forward an index, with roll over.
        ///</Summary>
        ///<param name="pushAmount">
        ///The amount of times the children will be pushed.
        ///</param>
        ///<returns>
        ///The base element.
        ///</returns>
        public static VisualElement PushChildrenForward(this VisualElement element, int pushAmount = 1)
        {
            for (int i = 0; i < pushAmount; i++)
            {
                for (int i2 = 0; i2 < element.childCount - 1; i2++)
                {
                    element.Add(element.ElementAt(0));
                }
            }
            return element;
        }

        ///<Summary>
        ///Finds a serialized property by name and draws its default field
        ///</Summary>
        ///<returns>
        ///The new element.
        ///</returns>
        public static IMGUIContainer CreatePropertyField(this SerializedObject sObject, string name)
        {
            return new IMGUIContainer(() =>
            {
                sObject.Update();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(sObject.FindPropertyOrFail(name));
                if (EditorGUI.EndChangeCheck())
                {
                    sObject.ApplyModifiedProperties();
                }
            });
        }


        public static IMGUIContainer CreatePropertyRelativeField(this SerializedProperty sObject, string name)
        {
            return new IMGUIContainer(() =>
            {
                sObject.serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(sObject.FindPropertyRelativeOrFail(name));
                if (EditorGUI.EndChangeCheck())
                {
                    sObject.serializedObject.ApplyModifiedProperties();
                }
            });
        }

        public static SerializedObject GetObjectReference(this SerializedProperty property)
        {
            if (property.GetUnderlyingValue() != null)
            {
                return new SerializedObject(EditorHelper.GetTargetObjectOfProperty(property) as UnityEngine.Object);
            }
            return null;
        }

        public static VisualElement LoadFromUXML(this VisualElement element, string address)
        {
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(address).CloneTree(element);
            return element;
        }


        ///<Summary>
        ///Finds the value of a scripatable variable reference depending on whether it is using the constant or not
        ///</Summary>
        ///<returns>
        ///The final value of the reference
        ///</returns>
        public static SerializedProperty FindVariableReferenceValueProperty(this SerializedProperty property, SerializedObject variableObject)
        {
            return property.FindVariableReferenceValueProperty(variableObject, "_constantValue", "Value");
        }

        ///<Summary>
        ///Finds the value of a scripatable variable reference depending on whether it is using the constant or not
        ///</Summary>
        ///<Param name="variableObject">
        ///The serializable object of the variable
        ///</Param>
        ///<Param name="constantName">
        ///The name of the constant value in the reference
        ///</Param>
        ///<Param name="variableValueName">
        ///The name of the value in the variable
        ///</Param>
        ///<returns>
        ///The final value of the reference
        ///</returns>
        public static SerializedProperty FindVariableReferenceValueProperty(this SerializedProperty property, SerializedObject variableObject, string constantName, string variableValueName)
        {
            SerializedProperty useConstant = property.FindPropertyRelative("_useConstant");
            if (useConstant == null)
            {
                Debug.LogError("Property in VariableValue was not a ScriptableVariable");
                return null;
            }

            if (useConstant.boolValue || variableObject == null)
            {
                return property.FindPropertyRelativeOrFail(constantName);
            }
            else
            {
                return variableObject.FindPropertyOrFail(variableValueName);
            }
        }

        public static void UpdateConfig(VisualElement basePanel, ChangeEvent<UnityEngine.Object> x = null)
        {
            if (x.newValue == x.previousValue) { return; }
            if (basePanel.childCount > 1)
            {
                for (int i = 0; i < basePanel.ElementAt(1).childCount; i++)
                {
                    basePanel.ElementAt(1).RemoveAt(0);
                }
                basePanel.RemoveAt(1);
            }
            AddSections(basePanel, x.newValue);
        }

        public static void AddSections(VisualElement basePanel, UnityEngine.Object config)
        {
            if (config == null) { return; }
            var sections = UnityEditor.Editor.CreateEditor(config).CreateInspectorGUI();
            sections.RemoveAt(0);
            basePanel.LinkedAdd(sections);
        }

        public static SerializedProperty FindPreviousProperty(this SerializedProperty property)
        {
            SerializedObject serializedObject = property.serializedObject;
            SerializedProperty dummyProperty = null;
            string[] propertyNames = property.PropertyPathParts();
            if (propertyNames.Length <= 1)
            {
                return null;
            }
            for (int i = 0; i < property.PropertyPathParts().Length - 1; i++)
            {
                if (dummyProperty == null)
                {
                    dummyProperty = serializedObject.FindParsedProperty(propertyNames[i]);
                }
                else
                {
                    dummyProperty = dummyProperty.FindParsedPropertyRelative(propertyNames[i]);
                }
            }
            return dummyProperty;
        }

        static SerializedProperty FindParsedProperty(this SerializedObject serializedObject, string propertyName)
        {
            if (propertyName[propertyName.Length - 1] == ']')
            {
                string updatedArrayName = "";

                for (int i2 = 0; i2 < propertyName.Length - 3; i2++)
                {
                    updatedArrayName += propertyName[i2];
                }
                int arrayIndex = int.Parse(propertyName[propertyName.Length - 2].ToString());
                return serializedObject.FindPropertyOrFail(updatedArrayName).GetArrayElementAtIndex(arrayIndex);
            }
            else
            {
                return serializedObject.FindPropertyOrFail(propertyName);
            }
        }

        static SerializedProperty FindParsedPropertyRelative(this SerializedProperty property, string propertyName)
        {
            if (propertyName[propertyName.Length - 1] == ']')
            {
                string updatedArrayName = "";

                for (int i2 = 0; i2 < propertyName.Length - 3; i2++)
                {
                    updatedArrayName += propertyName[i2];
                }
                int arrayIndex = int.Parse(propertyName[propertyName.Length - 2].ToString());
                return property.FindPropertyRelativeOrFail(updatedArrayName).GetArrayElementAtIndex(arrayIndex);
            }
            else
            {
                return property.FindPropertyRelativeOrFail(propertyName);
            }
        }

        public static IEnumerable<SerializedProperty> FindChildrenProperties(this SerializedProperty parent, int depth = 1)
        {
            var depthOfParent = parent.depth;
            var enumerator = parent.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current is not SerializedProperty childProperty) continue;
                if (childProperty.depth > depthOfParent + depth) continue;

                yield return childProperty.Copy();
            }
        }

        public static string GetBeginningOfPackagePath(string loaderAssetPath, string packageName)
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(loaderAssetPath);
            string startPath;
            if (packageInfo == null)
            {
                loaderAssetPath = loaderAssetPath.Replace('\\', '/');
                
                packageName = packageName.ReverseString();
                loaderAssetPath = loaderAssetPath.ReverseString();

                int endIndex = loaderAssetPath.IndexOf(packageName);
                int startIndex = loaderAssetPath.IndexOf("/stessA/", endIndex + packageName.Length);

                loaderAssetPath = loaderAssetPath.ReverseString();

                int offset = "/stessA/".Length;

                int subStringStart = loaderAssetPath.Length - (startIndex + offset - 1);
                startPath = loaderAssetPath.Substring(subStringStart/* - "/slaitnessEytilitU".Length*/, loaderAssetPath.Length - endIndex - subStringStart);
            }
            else
            {
                startPath = $"Packages/{packageName}";
            }

            return startPath;
        }

        public static string ReverseString(this string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            s = new(charArray);
            return s;
        }
    }


    public class LabeledButton : Button
    {
        public LabeledButton(Action clickEvent, string label) : base(clickEvent)
        {
            this.style.flexGrow = 1;
            this.Add(new Label(label));
        }
    }

    public class Row : VisualElement
    {
        public Row()
        {
            this.style.flexDirection = FlexDirection.Row;
        }
    }

    public static class EditorHelper
    {
        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.progress;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
    }
}
