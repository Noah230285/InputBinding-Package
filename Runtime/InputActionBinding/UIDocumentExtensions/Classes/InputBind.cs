using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UtilEssentials.InputIconBinding;
using UtilEssentials.InputIconBinding.VisualElements;
namespace UtilEssentials.InputActionBinding.UIDocumentExtenderer
{
    /// <summary>
    /// Allows the overriding of a specific, chosen, binding within an InputActionReference
    /// </summary>
    [Serializable]
    public class InputBind
    {
        UXMLRemmappingBinder _rootRemapper;
        Button _button;
        InputBindingIconElement _icon;
        public UXMLRemmappingBinder rootRemapper { set { _rootRemapper = value; } }

        [SerializeField] InputActionReference _actionReference;
        public InputActionReference actionReference => _actionReference;

        [SerializeField] int _bindingIndex;
        public int bindingIndex => _bindingIndex;

        [SerializeField] string _bindingID;
        public string bindingID => _bindingID;

        bool _hovered;
        enum UsabilityState
        {
            enabled,
            disabled,
            bricked,
        }
        UsabilityState _usabilityState;

        const string k_emptyText = "EMPTY";
        string _inputName;
        string _bindType;
        InputBindingCategories _bindCategory;


        InputActionRebindingExtensions.RebindingOperation _rebindOperation;

        // Initialisation and disabling
        #region

        /// <summary>
        /// Inside a given VisualElement, find a child Button with name <param name="buttonName"/> and bind to it
        /// </summary>
        /// <param name="inputRemapper"></param>
        /// <param name="buttonName"></param>
        /// <param name="inputName"></param>
        public void BindToUXML(VisualElement inputRemapper, string buttonName, string inputName)
        {
            if (_actionReference == null)
            {
                return;
            }

            _button = inputRemapper.Q(buttonName).ElementAt(0) as Button;
            _icon = _button.Q<InputBindingIconElement>("Icon");

            // If the chosen binding option was 'disabled'
            if (bindingIndex == -1)
            {
                _button.focusable = false;
                _button.AddToClassList("disabled");
                _usabilityState = UsabilityState.bricked;
                return;
            }

            _button.RegisterCallback<MouseEnterEvent>(MouseEnter);
            _button.RegisterCallback<MouseLeaveEvent>(MouseLeave);
            _button.RegisterCallback<FocusInEvent>(Selected);
            _button.RegisterCallback<FocusOutEvent>(UnSelected);
            _button.clicked += MouseClick;

            _inputName = inputName;
            switch (buttonName)
            {
                case InputActionBinder.k_primaryButtonName:
                    _bindType = "primary";
                    break;
                case InputActionBinder.k_secondaryButtonName:
                    _bindType = "secondary";
                    break;
                case InputActionBinder.k_controllerButtonName:
                    _bindType = "controller";
                    break;
                default: break;
            }

            if (!(_actionReference.action.bindings.Count - 1 >= bindingIndex))
            {
                Debug.LogError($"Binding index overflow {_actionReference} {buttonName}", _rootRemapper);
            }

            SetBindingCategory();
            UpdateButton();
        }

        /// <summary>
        /// Unbind all the bound methods to prevent errors
        /// </summary>
        public void UnbindEvents()
        {
            if (_hovered)
            {
                _hovered = false;
                _rootRemapper.emptyBindingActionReference.action.performed -= EmptyBinding;
                _rootRemapper.resetBindingActionReference.action.performed -= ResetBinding;
            }

            _button.UnregisterCallback<MouseEnterEvent>(MouseEnter);
            _button.UnregisterCallback<MouseLeaveEvent>(MouseLeave);
            _button.UnregisterCallback<FocusInEvent>(Selected);
            _button.UnregisterCallback<FocusOutEvent>(UnSelected);
            _button.clicked -= MouseClick;
        }
        #endregion

        // Proxy methods to be bound
        #region
        void MouseEnter(MouseEnterEvent evt)
        {
            EnterHover();
        }
        void MouseLeave(MouseLeaveEvent evt)
        {
            LeaveHover();
        }
        void Selected(FocusInEvent evt)
        {
            EnterHover();
        }
        void UnSelected(FocusOutEvent evt)
        {
            LeaveHover();
        }
        void MouseClick()
        {
            StartInteractiveRebind();
        }
        #endregion

        // Binds and unbinds actions when hovered and unhovered respectively
        #region

        void EnterHover()
        {
            if (_hovered) return;

            _rootRemapper.ReplaceHoverTarget(this);
            _hovered = true;

            if (_usabilityState == UsabilityState.enabled)
            {
                _rootRemapper.emptyBindingActionReference.action.performed += EmptyBinding;
                _rootRemapper.resetBindingActionReference.action.performed += ResetBinding;
            }
        }

        public void LeaveHover()
        {
            if (!_hovered) return;
            _hovered = false;
            if (_usabilityState == UsabilityState.enabled)
            {
                _rootRemapper.emptyBindingActionReference.action.performed -= EmptyBinding;
                _rootRemapper.resetBindingActionReference.action.performed -= ResetBinding;
            }
        }

        #endregion

        // Enables and disables the UI Button through greying it out
        #region

        public void EnableButton()
        {
            if (_usabilityState == UsabilityState.bricked)
            {
                return;
            }
            _usabilityState = UsabilityState.enabled;
            _button.RemoveFromClassList("greyedOut");
        }

        public void DisableButton()
        {
            if (_usabilityState == UsabilityState.bricked)
            {
                return;
            }
            if (_hovered)
            {
                _rootRemapper.emptyBindingActionReference.action.performed -= EmptyBinding;
                _rootRemapper.resetBindingActionReference.action.performed -= ResetBinding;
            }
            _usabilityState = UsabilityState.disabled;
            _button.AddToClassList("greyedOut");
        }
        #endregion

        /// <summary>
        /// Empties this binding
        /// </summary>
        void EmptyBinding(InputAction.CallbackContext callback)
        {
            _rootRemapper.localRebind = true;
            _actionReference.action.ApplyBindingOverride(bindingIndex, "");
            UpdateButton();
        }

        /// <summary>
        /// Resets this binding to its initial state
        /// </summary>
        void ResetBinding(InputAction.CallbackContext callback)
        {
            InputAction action = _actionReference.action;
            InputBinding newBinding = action.bindings[bindingIndex];

            // If the overridden bind path is the same as the base one, do nothing
            if (newBinding.overridePath == newBinding.path)
            {
                return;
            }

            // Remove this binding override
            _rootRemapper.localRebind = true;
            string oldOverridePath = newBinding.overridePath;
            if (oldOverridePath == null)
            {
                oldOverridePath = "";
            }

            action.RemoveBindingOverride(bindingIndex);

            // If the base/new binding is empty, stop here
            if (newBinding.path == "")
            {
                UpdateButton();
                return;
            }

            // Search for any duplicate inputs that might have been created, if found swap their input with the previous override binding value
            if (_rootRemapper.SearchForDuplicates(this, out var otherBinding))
            {
                otherBinding.actionReference.action.ApplyBindingOverride(otherBinding.bindingIndex, oldOverridePath);
                otherBinding.UpdateButton();
            }
            UpdateButton();
        }

        /// <summary>
        /// Updates the text or icon of this binder's button when the binding is changed
        /// </summary>
        public void UpdateButton()
        {
            if (bindingIndex == -1)
            {
                return;
            }

            var action = _actionReference?.action;
            if (action == null)
            {
                return;
            }
            var displayString = string.Empty;
            var deviceLayoutName = default(string);
            var controlPath = default(string);

            // Get display string from action.
            displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, InputBinding.DisplayStringOptions.DontIncludeInteractions);

            if (displayString == "")
            {
                displayString = k_emptyText;
            }

            // Update the icon for this binding path

            string bindingPath = action.bindings[bindingIndex].effectivePath;
            if (_icon != null && bindingPath != null && bindingPath != string.Empty)
            {
                // Update the InputBindingIconElement with the new binding information 
                _icon.inputBindingPath = $"{action.bindings[bindingIndex].effectivePath}";
                _icon.defaultText = displayString;
                _icon.bindingCategory = _bindCategory;
                _icon.UpdateIcon();
                return;
            }

            // Update the InputBindingIconElement to remove the image and show the display string text
            _icon.inputBindingPath = null;
            _icon.defaultText = displayString;
            _icon.UpdateIcon();
        }

        /// <summary>
        /// Check if rebind is possible before performing one
        /// </summary>
        public void StartInteractiveRebind()
        {
            if (_usabilityState != UsabilityState.enabled)
            {
                return;
            }

            if (_rootRemapper.localRebind || _rootRemapper.doingRebinding)
            {
                return;
            }

            PerformInteractiveRebind(actionReference?.action, _bindingIndex);
        }

        /// <summary>
        /// Initiate an interactive rebind that lets the player choose a new binding for the action.
        /// for the action.
        /// </summary>
        void PerformInteractiveRebind(InputAction action, int bindingIndex)
        {
            // Cancels current rebind operation if one is active
            _rebindOperation?.Cancel();

            action.Disable();

            // Action ran when the rebind is complete or canceled
            Action<InputActionRebindingExtensions.RebindingOperation> cleanUp = (operation) =>
            {
                _rootRemapper.OnRebindStopEvent(operation);
                _rootRemapper.DisableOverlay();

                _rebindOperation?.Dispose();
                _rebindOperation = null;
                _rootRemapper.doingRebinding = false;
                _rootRemapper.localRebind = true;
                SetBindingCategory();


                UpdateButton();

                // Small delay on re-enabling rebinding to prevent errors
                _rootRemapper.StartCoroutine(EnableRebindingDelay());
                IEnumerator EnableRebindingDelay()
                {
                    yield return null;
                    action.Enable();
                    _rootRemapper.localRebind = false;
                }
            };

            // Configure the rebind
            _rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .OnCancel(cleanUp)
                .OnComplete(operation =>
                {
                    // If the new binding is duplicated somewhere else, empty that duplicate's binding
                    if (_rootRemapper.SearchForDuplicates(this, out InputBind duplicateBind))
                    {
                        duplicateBind.EmptyBinding(new InputAction.CallbackContext());
                    }
                    cleanUp(operation);
                });

            // Find the binding path for exiting for the current control method
            InputAction exitAction = _rootRemapper.exitBindingPromptActionReference.action;
            InputBinding exitBind = new();
            string exitBindName = null;

            for (int i = 0; i < exitAction.bindings.Count; i++)
            {
                var bind = exitAction.bindings[i];
                if (bind.isPartOfComposite)
                {
                    continue;
                }
                if (_rootRemapper.UIDocumentExtender.controlScheme.Equals(bind.groups))
                {
                    exitBind = bind;
                    exitBindName = exitAction.GetBindingDisplayString(i);
                    break;
                }
            }

            if (exitBindName != null)
            {
                _rebindOperation.WithCancelingThrough(exitBind.effectivePath);
            }

            // Bring up rebind overlay
            _rootRemapper.EnableOverlay(_bindType, _inputName, exitBind.effectivePath, _bindCategory, exitBindName);
            _rootRemapper.localRebind = true;
            _rootRemapper.doingRebinding = true;

            _rootRemapper.OnRebindStartEvent(_rebindOperation);

            _rebindOperation.Start();
        }

        void SetBindingCategory()
        {
            string bindingPath = _actionReference.action.bindings[bindingIndex].effectivePath;
            if (bindingPath == null || bindingPath == string.Empty)
            {
                return;
            }

            int endIndex = bindingPath.IndexOf('>');
            string categoryPath = bindingPath.Substring(1, endIndex - 1);

            switch (categoryPath)
            {
                case "Gamepad":
                    _bindCategory = InputBindingCategories.Gamepad;
                    break;
                case "Keyboard":
                    _bindCategory = InputBindingCategories.Keyboard;
                    break;
                case "Mouse":
                    _bindCategory = InputBindingCategories.Mouse;
                    break;
                default:
                    _bindCategory = InputBindingCategories.Gamepad;
                    break;
            }
        }
    }
}