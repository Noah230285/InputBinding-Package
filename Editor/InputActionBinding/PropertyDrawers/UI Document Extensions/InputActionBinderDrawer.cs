using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UtilEssentials.InputActionBinding.UIDocumentExtenderer;
using UtilEssentials.UIToolkitUtility.Editor.VisualElements;

namespace UtilEssentials.InputActionBinding.UIToolkit
{
    /// <summary>
    /// Property Drawer for the class InputActionBinder <see cref="UXMLRemmappingBinder.InputActionBinder"/>
    /// </summary>
    [CustomPropertyDrawer(typeof(InputActionBinder))]
    public class InputActionBinderDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var inputName = property.FindPropertyRelative("_inputName");
            var actionProperty = property.FindPropertyRelative("_actionReference");
            var extendedProperty = property.FindPropertyRelative("_sectionExtended");

            var root = new SectionElement(inputName.stringValue, extendedProperty);

            // New Property Field 'nameField' bound to the name of the input and the title of this section grouping
            var nameField = new PropertyField(inputName);
            nameField.RegisterValueChangeCallback(x =>
            {
                root.label.text = x.changedProperty.stringValue;
            });
            root.LinkedAddContent(nameField);

            // Creates fields for an input action reference, and three bindings
            var actionField = new PropertyField(actionProperty);
            root
            .LinkedAddContent(actionField)
            .LinkedAddContent(new InputBindElement(actionField, actionProperty, property.FindPropertyRelative("_primary")))
            .LinkedAddContent(new InputBindElement(actionField, actionProperty, property.FindPropertyRelative("_secondary")))
            .LinkedAddContent(new InputBindElement(actionField, actionProperty, property.FindPropertyRelative("_controller")));

            return root;
        }

        /// <summary>
        /// PopupField for choosing one of an Input Action's bindings
        /// </summary>
        public class InputBindElement : VisualElement
        {
            SerializedProperty _actionProperty;
            SerializedProperty _bindingIdProperty;
            SerializedProperty _bindingIndexProperty;

            const string _disableBind = "Disable Bind";
            private string[] _BindingOptionIDs;

            /// <param name="actionField"> PropertyField of an an InputActionReference </param>
            /// <param name="actionProperty"> The bound SerializedProperty in actionField <param name="actionField"/></param>
            /// <param name="inputBindProperty"> SerializedProperty of an InputBind <see cref="UXMLRemmappingBinder.InputBind"/></param>
            public InputBindElement(PropertyField actionField, SerializedProperty actionProperty, SerializedProperty inputBindProperty)
            {
                _actionProperty = inputBindProperty.FindPropertyRelative("_actionReference");
                _bindingIdProperty = inputBindProperty.FindPropertyRelative("_bindingID");
                _bindingIndexProperty = inputBindProperty.FindPropertyRelative("_bindingIndex");

                _actionProperty.objectReferenceValue = actionProperty.objectReferenceValue;
                inputBindProperty.serializedObject.ApplyModifiedProperties();

                VisualElement root = this;

                FindBindingValues(out List<string> bindings, out string currentBinding);

                // PopupField for selecting bindings
                var popup = new PopupField<string>($"{inputBindProperty.displayName} Binding");
                popup.style.marginTop = 3;
                popup.choices = bindings;
                popup.value = currentBinding;

                // Option in the popup selected
                popup.formatSelectedValueCallback =
                    (x) =>
                    {
                        if (x == "")
                        {
                            return x;
                        }

                        if (x == _disableBind)
                        {
                            _bindingIdProperty.stringValue = "";
                            _bindingIndexProperty.intValue = -1;
                            inputBindProperty.serializedObject.ApplyModifiedProperties();
                            return x;
                        }

                        // Returns the binding index
                        int index = popup.choices.IndexOf(x) - 1;
                        if (index < 0)
                        {
                            return x;
                        }

                        _bindingIdProperty.stringValue = _BindingOptionIDs[index];
                        _bindingIndexProperty.intValue = index;
                        inputBindProperty.serializedObject.ApplyModifiedProperties();
                        return x;
                    };

                // Action for writing each line in popup
                popup.formatListItemCallback =
                    (x) =>
                    {
                        if (x == "skip")
                        {
                            return null;
                        }
                        return x;
                    };

                // When actionField is updated
                actionField.RegisterValueChangeCallback(x =>
                {
                    _actionProperty.objectReferenceValue = x.changedProperty.objectReferenceValue;
                    inputBindProperty.serializedObject.ApplyModifiedProperties();

                    // Hide the popup when there the reference is null, and repopulate it when the reference is valid
                    if (x.changedProperty.objectReferenceValue == null)
                    {
                        popup.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        FindBindingValues(out List<string> bindings, out string currentBinding);
                        popup.choices = bindings;
                        popup.value = currentBinding;
                        popup.style.display = DisplayStyle.Flex;
                    }
                });

                if (bindings == null)
                {
                    popup.style.display = DisplayStyle.None;
                }

                root.Add(popup);
            }

            /// <summary>
            /// Return the list of all the bindings in the bound InputActionReference and the currently selected binding by this element, if any
            /// </summary>
            /// <param name="bindingsList">Out variable that return's the list of all the bindings in the bound InputActionReference</param>
            /// <param name="selectedBindingOption">The currently selected binding by this element, if any</param>
            void FindBindingValues(out List<string> bindingsList, out string selectedBindingOption)
            {
                selectedBindingOption = "";
                bindingsList = new List<string>();
                var actionReference = (InputActionReference)_actionProperty.objectReferenceValue;
                var action = actionReference?.action;

                if (action == null)
                {
                    bindingsList.Add("");
                    _BindingOptionIDs = new string[0];
                    return;
                }

                // Adds the 'disable bind' option to the list of bindings
                bindingsList.Add(_disableBind);
                selectedBindingOption = _disableBind;


                var bindings = action.bindings;
                int bindingCount = bindings.Count;
                string currentBindingId = _bindingIdProperty.stringValue;

                _BindingOptionIDs = new string[bindings.Count];

                // Iterates through each binding and adds it to 'bindingsList'
                for (var i = 0; i < bindingCount; ++i)
                {
                    var currentBinding = bindings[i];

                    // If the binding is the start of a composite, ignore it and add a seperator to the list
                    if (currentBinding.isComposite)
                    {
                        bindingsList.Add("skip");
                        continue;
                    }

                    var bindingId = currentBinding.id.ToString();
                    var haveBindingGroups = !string.IsNullOrEmpty(currentBinding.groups);

                    // If we don't have a binding groups (control schemes), show the device that if there are, for example,
                    // there are two bindings with the display string "A", the user can see that one is for the keyboard
                    // and the other for the gamepad.
                    var displayOptions =
                        InputBinding.DisplayStringOptions.DontUseShortDisplayNames | InputBinding.DisplayStringOptions.IgnoreBindingOverrides;
                    if (!haveBindingGroups)
                        displayOptions |= InputBinding.DisplayStringOptions.DontOmitDevice;

                    // Create display string.
                    var displayString = action.GetBindingDisplayString(i, displayOptions);
                    if (displayString == "")
                    {
                        displayString = $"Unbound {i}";
                    }

                    // If binding is part of a composite, include the part name.
                    if (currentBinding.isPartOfComposite)
                        displayString = $"{ObjectNames.NicifyVariableName(currentBinding.name)}: {displayString}";

                    // Some composites use '/' as a separator. When used in popup, this will lead to to submenus. Prevent
                    // by instead using a backlash.
                    displayString = displayString.Replace('/', '\\');

                    // If the binding is part of control schemes, mention them.
                    if (haveBindingGroups)
                    {
                        var asset = action.actionMap?.asset;
                        if (asset != null)
                        {
                            var controlSchemes = string.Join(", ",
                                currentBinding.groups.Split(InputBinding.Separator)
                                    .Select(x => asset.controlSchemes.FirstOrDefault(c => c.bindingGroup == x).name));

                            displayString = $"{displayString} ({controlSchemes})";
                        }
                    }

                    // Registers the binding into the bindings selector list and ID array
                    bindingsList.Add(displayString);
                    _BindingOptionIDs[i] = bindingId;

                    if (currentBindingId == bindingId)
                        selectedBindingOption = displayString;
                }
            }

        }
    }
}
