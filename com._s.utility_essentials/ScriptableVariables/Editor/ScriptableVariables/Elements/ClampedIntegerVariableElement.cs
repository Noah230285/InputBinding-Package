using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UtilEssentials.UIToolkitUtility.Editor;

namespace UtilEssentials.ScriptableVariables.Editor
{
    public class ClampedIntegerElement : VisualElement
    {
        Button _optionsButton;
        Label _label;
        ProgressBar _bar;
        Slider _slider;
        FloatField _currentField;
        FloatField _maxField;
        VisualElement _options;
        Button _constantButton;
        VisualElement _constantButtonIcon;
        ObjectField _eventField;
        ObjectField _variableField;
        IntegerReferenceField _currentReferenceField;
        IntegerReferenceField _maxReferenceField;

        SerializedProperty _thisVariableProperty;
        SerializedProperty _configExtendedProperty;
        SerializedProperty _useConstantProperty;

        SerializedProperty _variableProperty;
        SerializedProperty _currentVariableProperty;
        SerializedProperty _currentProperty;
        SerializedProperty _maxVariableProperty;
        SerializedProperty _maxProperty;
        SerializedProperty _eventChannelProperty;

        SerializedObject _serializedObject;
        ClampedIntegerReference _root;

        Texture2D _locked;
        Texture2D _unlocked;

        #region
        public new class UxmlFactory : UxmlFactory<ClampedIntegerElement> { }
        public ClampedIntegerElement()
        {
            Inititialize();
        }
        #endregion

        public ClampedIntegerElement(SerializedProperty property)
        {
            Inititialize();
            Configure(property);
        }

        void Inititialize()
        {
            string assetPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            string beginingPath = UIToolkitUtilityFunctions.GetBeginningOfPackagePath(assetPath, "com._s.utility_essentials");

            _locked = AssetDatabase.LoadAssetAtPath<Texture2D>($"{beginingPath}/ScriptableVariables/Assets/Icons/locked.png");
            _unlocked = AssetDatabase.LoadAssetAtPath<Texture2D>($"{beginingPath}/ScriptableVariables/Assets/Icons/Unlocked.png");

            this.LoadFromUXML($"{beginingPath}/ScriptableVariables/Assets/UIToolkit/UXML/Templates/ClampedIntegerUXML.uxml");

            _optionsButton = this.Q<Button>("OptionsButton");
            _label = this.Q<Label>("Title");
            _bar = this.Q<ProgressBar>();
            _slider = this.Q<Slider>();
            _currentField = this.Q<FloatField>("CurrentField");
            _maxField = this.Q<FloatField>("MaxField");
            _options = this.Q<VisualElement>("Options");
            _constantButton = this.Q<Button>("ConstantButton");
            _constantButtonIcon = _constantButton.ElementAt(0);
            _eventField = this.Q<ObjectField>("EventField");
            _variableField = this.Q<ObjectField>("ReferenceField");
            _currentReferenceField = this.Q<IntegerReferenceField>("CurrentVariable");
            _maxReferenceField = this.Q<IntegerReferenceField>("MaxVariable");
        }

        void Configure(SerializedProperty property)
        {
            // Find properties
            _serializedObject = property.serializedObject;
            _thisVariableProperty = property;

            _variableProperty = property.FindPropertyRelativeOrFail("_variable");
            _configExtendedProperty = property.FindPropertyRelativeOrFail("_configExtended");
            _useConstantProperty = property.FindPropertyRelativeOrFail("_useConstant");
            _eventChannelProperty = property.FindPropertyRelativeOrFail("_eventChannel");

            // Bind elements
            _label.text = property.displayName;
            _eventField.LinkedBindProperty(_eventChannelProperty);
            _variableField.LinkedBindProperty(_variableProperty);
            _variableField.RegisterValueChangedCallback(change => ConstantUpdated());

            _slider.RegisterValueChangedCallback((change) => OnProgressChanged(change));
            _constantButton.BindProperty(_useConstantProperty);
            _constantButton.clicked += () => _useConstantProperty.FlipFlopProperty();
            _constantButton.RegisterValueChangedCallback((change) => ConstantUpdated());
            _maxField.RegisterValueChangedCallback(change => MaxChanged(change));
            ConstantUpdated();

            _optionsButton.BindElementDisplay(_configExtendedProperty, _options);
            _currentReferenceField.UseConstantChanged += (b) => CurrentChanged();
            _maxReferenceField.UseConstantChanged += (b) => MaxChanged();
            _maxReferenceField.BindProperties(_maxVariableProperty);
        }

        void ConstantUpdated()
        {
            SerializedObject _variableObject = _variableProperty.GetUnderlyingValue() != null ? UIToolkitUtilityFunctions.GetObjectReference(_variableProperty) : null;
            _currentVariableProperty = _thisVariableProperty.FindVariableReferenceValueProperty(_variableObject, "_constantCurrent", "_current");
            _currentReferenceField.BindProperties(_currentVariableProperty);
            _maxVariableProperty = _thisVariableProperty.FindVariableReferenceValueProperty(_variableObject, "_constantMax", "_max");
            _maxReferenceField.BindProperties(_maxVariableProperty);
            CurrentChanged();
            MaxChanged();

            if (_useConstantProperty.boolValue)
            {
                _variableField.AddToClassList("halfTransparent");
                _variableField.SetEnabled(false);
                _constantButtonIcon.style.backgroundImage = _unlocked;
            }
            else
            {
                _variableField.RemoveFromClassList("halfTransparent");
                _variableField.SetEnabled(true);
                _constantButtonIcon.style.backgroundImage = _locked;
            }
        }

        void CurrentChanged()
        {
            SerializedObject _currentObject = _currentVariableProperty.FindPropertyRelative("_variable").GetUnderlyingValue() != null ? UIToolkitUtilityFunctions.GetObjectReference(_currentVariableProperty.FindPropertyRelative("_variable")) : null;
            _currentProperty = _currentVariableProperty.FindVariableReferenceValueProperty(_currentObject);
            _currentField.BindProperty(_currentProperty);
            _bar.BindProperty(_currentProperty);
            _slider.BindProperty(_currentProperty);
        }

        void MaxChanged()
        {
            SerializedObject _maxObject = _maxVariableProperty.FindPropertyRelative("_variable").GetUnderlyingValue() != null ? UIToolkitUtilityFunctions.GetObjectReference(_maxVariableProperty.FindPropertyRelative("_variable")) : null;
            _maxProperty = _maxVariableProperty.FindVariableReferenceValueProperty(_maxObject);
            _maxField.LinkedBindProperty(_maxProperty);
        }

        bool OnProgressChanged(ChangeEvent<float> change)
        {
            //Debug.Log($"change attempt {change.newValue} {change.previousValue} {_progressReference.floatValue}");
            if (change.newValue == change.previousValue || change.newValue == 0) { return false; }
            //Debug.Log("change success");
            _currentProperty.floatValue = change.newValue;
            if (Application.isPlaying)
            {
                (_eventChannelProperty.objectReferenceValue as ScriptableEventChannel)?.OnRaiseEvents();
            }
            _serializedObject.ApplyModifiedProperties();
            return true;
            //if (_cooldown.StartTick()) { _cooldownReference.FindPropertyRelative("_ticking").boolValue = true; }
            //_cooldownReference.serializedObject.ApplyModifiedProperties();
        }

        void MaxChanged(ChangeEvent<float> change)
        {
            //Debug.Log($"Max change {change.newValue} {change.previousValue} {_progressReference.floatValue}");
            _bar.highValue = change.newValue;
            _slider.highValue = change.newValue;
            //if (change.newValue == _cooldown.Max) { return; }
            //_cooldown.StartTick();
            //_cooldownReference.serializedObject.ApplyModifiedProperties();
        }
    }
}