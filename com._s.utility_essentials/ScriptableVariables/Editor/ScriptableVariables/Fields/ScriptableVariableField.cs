using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using UtilEssentials.UIToolkitUtility.Editor;
using Unity.VisualScripting;

namespace UtilEssentials.ScriptableVariables.Editor
{
    internal abstract class ScriptableVariableField<T> : BaseField<T>
    {
        VisualElement _contentElement;
        Button _buttonElement;
        VisualElement _buttonIconElement;
        VisualElement _fieldConnectorElement;
        protected ObjectField _objectField;

        BaseField<T> _variableField;

        SerializedProperty _referenceProperty;
        SerializedProperty _useConstantProperty;
        SerializedProperty _variableProperty;
        SerializedProperty _valueProperty;

        public event Action<bool> UseConstantChanged;

        string _beginingPath;
        Texture2D _locked;
        Texture2D _unlocked;

        public ScriptableVariableField(SerializedProperty variableReferenceProperty, string label = "Empty") : base(label, null)
        {
            UseConstantChanged?.Invoke(false);
            Init(variableReferenceProperty);
        }

        // Initialise the element
        public void Init(SerializedProperty reference = null)
        {
            //Find the path for this package
            string assetPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            string beginingPath = UIToolkitUtilityFunctions.GetBeginningOfPackagePath(assetPath, "com._s.utility_essentials");

            _locked = AssetDatabase.LoadAssetAtPath<Texture2D>($"{beginingPath}/ScriptableVariables/Assets/Icons/locked.png");
            _unlocked = AssetDatabase.LoadAssetAtPath<Texture2D>($"{beginingPath}/ScriptableVariables/Assets/Icons/Unlocked.png");

            this.LoadFromUXML($"{beginingPath}/ScriptableVariables/Assets/UIToolkit/UXML/Templates/ScriptableVariable.uxml");
            this.RemoveAt(0);
            labelElement.AddToClassList("label");
            this.AddToClassList("alignCenter");
            _buttonElement = this.Q<Button>();
            _buttonIconElement = _buttonElement.ElementAt(0);
            _contentElement = this.Q<VisualElement>("Content");
            _fieldConnectorElement = this.Q<VisualElement>("Connector");
            _objectField = this.Q<ObjectField>("ReferenceField");

            _variableField = CreateField();

            _buttonElement.clicked += () => _useConstantProperty.FlipFlopProperty();
            _buttonElement.RegisterValueChangedCallback((change) => UpdateUseConstant());

            _contentElement.LinkedAdd(_variableField, 0);
            this.LinkedAdd(_variableField.labelElement, 0);
            if (reference != null) { BindProperties(reference); }
        }

        public void BindProperties(SerializedProperty reference, string label = null)
        {
            reference.serializedObject.Update();
            (this.ElementAt(0) as Label).text = label == null ? reference.displayName : label;
            _referenceProperty = reference;
            _useConstantProperty = _referenceProperty.FindPropertyRelativeOrFail("_useConstant");
            _variableProperty = _referenceProperty.FindPropertyRelativeOrFail("_variable");
            SerializedObject _variableObject = _variableProperty.objectReferenceValue != null ? new SerializedObject(_variableProperty.objectReferenceValue) : null;
            _valueProperty = _referenceProperty.FindVariableReferenceValueProperty(_variableObject);
            _objectField.BindProperty(_referenceProperty.FindPropertyRelativeOrFail("_variable"));

            // Bind the the selected scriptable variable field
            _objectField.objectType = _variableProperty.GetUnderlyingType();
            _objectField.RegisterValueChangedCallback(x => ObjectChanged(x));
            _buttonElement.BindProperty(_useConstantProperty);

            UpdateUseConstant();
            if (_variableProperty.objectReferenceValue != null)
            {
                _variableObject.ApplyModifiedProperties();
            }
            reference.serializedObject.ApplyModifiedProperties();
        }

        protected abstract BaseField<T> CreateField();

        void UpdateUseConstant()
        {
            SerializedObject _variableObject = _variableProperty.objectReferenceValue != null ? new SerializedObject(_variableProperty.objectReferenceValue) : null;
            _valueProperty = _referenceProperty.FindVariableReferenceValueProperty(_variableObject);
            _variableField.BindProperty(_valueProperty);

            if (_useConstantProperty.boolValue) // Use Constant
            {
                if (_variableProperty.objectReferenceValue != null)
                {
                    _objectField.AddToClassList("hidden");
                }
                _fieldConnectorElement.AddToClassList("hidden");
                _variableField.AddToClassList("variableField");
                _buttonIconElement.style.backgroundImage = _unlocked;
            }
            else // Use Variable
            {
                _objectField.RemoveFromClassList("hidden");
                _fieldConnectorElement.RemoveFromClassList("hidden");
                _variableField.RemoveFromClassList("variableField");
                _buttonIconElement.style.backgroundImage = _locked;
            }
        }

        void ObjectChanged(ChangeEvent<UnityEngine.Object> changeEvent)
        {
            SerializedObject _variableObject = _variableProperty.objectReferenceValue != null ? new SerializedObject(_variableProperty.objectReferenceValue) : null;
            _valueProperty = _referenceProperty.FindVariableReferenceValueProperty(_variableObject);
            if (changeEvent.newValue == null)
            {
                _objectField.RemoveFromClassList("hidden");
                _buttonElement.AddToClassList("halfTransparent");
                _buttonElement.SetEnabled(false);
                _useConstantProperty.boolValue = true;
                _objectField.style.marginLeft = 20;
            }
            else if (changeEvent.previousValue == null)
            {
                _buttonElement.RemoveFromClassList("halfTransparent");
                _buttonElement.SetEnabled(true);
                _useConstantProperty.boolValue = false;
                _objectField.style.marginLeft = 0;
            }
            else
            {
                _variableField.BindProperty(_valueProperty);
                return;
            }
            UpdateUseConstant();
            if (_variableProperty.objectReferenceValue != null)
            {
                _variableObject.ApplyModifiedProperties();
            }
            _useConstantProperty.serializedObject.ApplyModifiedProperties();
        }

        public void BindProperty(SerializedProperty property)
        {
            _objectField.BindProperty(property);
        }

        public void SetReadOnly()
        {
            _variableField.LinkedReadOnly();
        }
    }
}
