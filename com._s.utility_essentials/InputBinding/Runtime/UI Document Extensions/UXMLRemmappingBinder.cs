using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UtilEssentials.UIDocumentExtenderer;

namespace UtilEssentials.InputActionBinding.UIDocumentExtenderer
{
    /// <summary>
    /// Allows the remapping of Input Action References in realtime via a UIToolkit interface
    /// Requires the attached Gameobject to have a UIDocumentExtender <see cref="UIDocumentExtender"> component
    /// </summary>
    [RequireComponent(typeof(UIDocumentExtender))]
    public class UXMLRemmappingBinder : MonoBehaviour
    {
        /// <summary>
        /// Takes a InputActionReference and allows the realtime binding of three bindings properties through the InputBind <see cref="InputBind"> class.
        /// This includes two Keyboard & Mouse binds, and one for Controller
        /// </summary>
        [Serializable]
        public class InputActionBinder
        {
            [SerializeField] string _inputName;
            [SerializeField] InputActionReference _actionReference;
            public InputActionReference actionReference => _actionReference;

            [SerializeField] InputBind _primary;
            public InputBind primary => _primary;
            public const string k_primaryButtonName = "KeyboardPrimary";

            [SerializeField] InputBind _secondary;
            public InputBind secondary => _secondary;
            public const string k_secondaryButtonName = "KeyboardSecondary";

            [SerializeField] InputBind _controller;
            public InputBind controller => _controller;
            public const string k_controllerButtonName = "Controller";

#if UNITY_EDITOR
            [SerializeField] bool _sectionExtended;
#endif
            /// <summary>
            /// Looks through this Gameobject's UIDocument to try and find a VisualElement with the provided name.
            /// If that element is found, try to find and bind its buttons through each InputBind <see cref="InputBind">
            /// </summary>
            /// <param name="document"></param>
            public void BindUXML(UIDocument document)
            {
                VisualElement InputRemapper = document.rootVisualElement.Q(_inputName);
                primary.BindToUXML(InputRemapper, k_primaryButtonName, _inputName);
                secondary.BindToUXML(InputRemapper, k_secondaryButtonName, _inputName);
                controller.BindToUXML(InputRemapper, k_controllerButtonName, _inputName);
            }

            /// <summary>
            /// Unbind from the UIDocument from each InputBind <see cref="InputBind">
            /// </summary>
            public void UnbindEvents()
            {
                primary.UnbindEvents();
                secondary.UnbindEvents();
                controller.UnbindEvents();
            }

            /// <summary>
            /// Switch between using Keyboard & Mouse inputs and Controller
            /// </summary>
            /// <param name="usingKeyboard"></param>
            public void ToggleKeyboardBinding(bool usingKeyboard)
            {
                if (usingKeyboard)
                {
                    primary.EnableButton();
                    secondary.EnableButton();
                    controller.DisableButton();
                }
                else
                {
                    primary.DisableButton();
                    secondary.DisableButton();
                    controller.EnableButton();
                }
            }
        }

        /// <summary>
        /// Allows the overriding of a specific, chosen, binding within an InputActionReference
        /// </summary>
        [Serializable]
        public class InputBind
        {
            UXMLRemmappingBinder _rootRemapper;
            Button _button;
            VisualElement _icon;
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
                _icon = _button.Q("Icon");

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

                if (_button != null)
                {
                    // Find the icon for this binding path
                    Texture2D icon = ButtonIconBindingsEditorSettings.GetTextureFromBindingPath($"{action.bindings[bindingIndex].effectivePath}");

                    if (icon == null)
                    {
                        _button.text = displayString;
                        _icon.style.backgroundImage = null;
                    }
                    else
                    {
                        _button.text = "";
                        _icon.style.backgroundImage = icon;
                    }
                }
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
                    if (_rootRemapper._UIDocumentExtender.controlScheme.Equals(bind.groups))
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
                _rootRemapper.EnableOverlay(_bindType, _inputName, exitBindName);
                _rootRemapper.localRebind = true;
                _rootRemapper.doingRebinding = true;

                _rootRemapper.OnRebindStartEvent(_rebindOperation);

                _rebindOperation.Start();
            }
        }

        public InputActionBinder[] _inputActionBinders;

        [SerializeField] InputActionReference _resetBindingActionReference;
        public InputActionReference resetBindingActionReference => _resetBindingActionReference;

        [SerializeField] InputActionReference _emptyBindingActionReference;
        public InputActionReference emptyBindingActionReference => _emptyBindingActionReference;

        [SerializeField] InputActionReference _exitBindingPromptActionReference;
        public InputActionReference exitBindingPromptActionReference => _exitBindingPromptActionReference;

        [Serializable]
        public class InteractiveUXMLRebindEvent : UnityEvent<UXMLRemmappingBinder, InputActionRebindingExtensions.RebindingOperation>
        {
        }

        [Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, "
        + "to implement custom UI behavior while a rebind is in progress. It can also be used to further "
        + "customize the rebind.")]
        [SerializeField]
        InteractiveUXMLRebindEvent _rebindStartEvent;
        public void OnRebindStartEvent(InputActionRebindingExtensions.RebindingOperation x)
        {
            _rebindStartEvent.Invoke(this, x);
        }

        [Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
        [SerializeField]
        InteractiveUXMLRebindEvent _rebindStopEvent;
        public void OnRebindStopEvent(InputActionRebindingExtensions.RebindingOperation x)
        {
            _rebindStartEvent.Invoke(this, x);
        }

        bool _localRebind;
        public bool localRebind
        {
            get => _localRebind;
            set { _localRebind = value; }
        }

        bool _doingRebinding;
        bool doingRebinding
        {
            get => _doingRebinding;
            set { _doingRebinding = value; }
        }

        UIDocument _UIDocument;
        UIDocumentExtender _UIDocumentExtender;
        public VisualElement focusedElement => _UIDocumentExtender.currentlyFocused;

        VisualElement _bindingOverlay;
        Label _topLabel;
        Label _bottomLabel;

        InputBind _currentBinding;


        void Awake()
        {
            _UIDocument = GetComponent<UIDocument>();
            _UIDocumentExtender = GetComponent<UIDocumentExtender>();
        }
        void OnEnable()
        {
            // Waits for any Async loading in the UIDocument
            if (!_UIDocumentExtender.isLoaded)
            {
                _UIDocumentExtender._UILoaded += Loaded;
                return;
            }
            Loaded();
        }

        /// <summary>
        /// This first loaded, after UIDocument loading is complete
        /// </summary>
        void Loaded()
        {
            for (int i = 0; i < _inputActionBinders.Length; i++)
            {
                _inputActionBinders[i].primary.rootRemapper = this;
                _inputActionBinders[i].secondary.rootRemapper = this;
                _inputActionBinders[i].controller.rootRemapper = this;
                _inputActionBinders[i].BindUXML(_UIDocument);
            }

            _bindingOverlay = _UIDocument.rootVisualElement.Q(null, "bindingOverlay");
            if (_bindingOverlay == null)
            {
                Debug.LogError($"UI Document {_UIDocument} does not contain element with class 'bindingOverlay'", this);
            }
            _topLabel = _bindingOverlay.Q("TopLabel") as Label;
            _bottomLabel = _bindingOverlay.Q("BottomLabel") as Label;

            InputSystem.onActionChange += ActionChanged;
            _UIDocumentExtender.UsingGamepad += UsingGamepad;
            _UIDocumentExtender.UsingKeyboard += UsingKeyboard;
        }

        void OnDisable()
        {
            for (int i = 0; i < _inputActionBinders.Length; i++)
            {
                _inputActionBinders[i].UnbindEvents();
            }

            InputSystem.onActionChange -= ActionChanged;
            _UIDocumentExtender.UsingGamepad -= UsingGamepad;
            _UIDocumentExtender.UsingKeyboard -= UsingKeyboard;
        }

        /// <summary>
        /// Updates the input binding buttons when an action is changed in case they are changed from an outside source, eg a different instance of this Behaviour
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="change"></param>
        public void ActionChanged(object obj, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged) return;
            if (_localRebind)
            {
                if (!doingRebinding)
                {
                    _localRebind = false;
                }
                return;
            }
            var action = obj as InputAction;
            var actionMap = action?.actionMap ?? obj as InputActionMap;
            var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

            foreach (var element in _inputActionBinders)
            {
                var referencedAction = element.actionReference.action;
                if (element.actionReference == null)
                {
                    continue;
                }

                if (referencedAction != action &&
                    referencedAction.actionMap != actionMap &&
                    referencedAction.actionMap?.asset != actionAsset)
                {
                    continue;
                }

                element.primary.UpdateButton();
                element.secondary.UpdateButton();
                element.controller.UpdateButton();
            }
            if (actionAsset == null)
            {
                return;
            }
        }

        /// <summary>
        /// When the control method is switched to Keyboard & mouse
        /// </summary>
        void UsingKeyboard()
        {
            foreach (var element in _inputActionBinders)
            {
                element.ToggleKeyboardBinding(true);
            }
        }

        /// <summary>
        /// When the control method is switched to Controller
        /// </summary>
        void UsingGamepad()
        {
            foreach (var element in _inputActionBinders)
            {
                element.ToggleKeyboardBinding(false);
            }
        }

        public void ReplaceHoverTarget(InputBind inputBind)
        {
            _currentBinding?.LeaveHover();
            _currentBinding = inputBind;
        }

        /// <summary>
        /// Enables the overlay for when a rebinding is in progress
        /// </summary>
        /// <param name="bindType"> Eg, primary, secondary, controller</param>
        /// <param name="actionName"> The name of the action that is being rebound</param>
        /// <param name="exitBinding">The name of the input binding that exits the binding</param>
        public void EnableOverlay(string bindType, string actionName, string exitBinding)
        {
            _bindingOverlay.RemoveFromClassList("hidden");
            _topLabel.text = $"Rebinding {bindType} input of '{actionName}'";

            if (exitBinding != null)
            {
                _bottomLabel.text = $"Press any input to register or press {exitBinding} to cancel";
            }
        }

        public void DisableOverlay()
        {
            _bindingOverlay.AddToClassList("hidden");
        }

        /// <summary>
        /// Searches for duplicate bindings paths of param<OriginRemap> and if it finds a duplicate, returns that InputBind param<searchForDuplicatesOut>
        /// </summary>
        /// <param name="OriginRemap"></param>
        /// <param name="searchForDuplicatesOut"></param>
        /// <returns>If duplicate was found</returns>
        bool SearchForDuplicates(InputBind OriginRemap, out InputBind searchForDuplicatesOut)
        {
            InputBinding OriginBinding = OriginRemap.actionReference.action.bindings[OriginRemap.bindingIndex];
            Func<InputActionBinder, InputBind, InputBind> compareBinding = (parentElement, compareInputBind) =>
            {
                if (compareInputBind == OriginRemap)
                {
                    return null;
                }
                int bindingIndex = compareInputBind.bindingIndex;
                InputAction compareAction = parentElement.actionReference.action;
                if (bindingIndex == -1)
                {
                    return null;
                }
                InputBinding compareBinding = compareAction.bindings[bindingIndex];

                if (compareBinding.effectivePath.Equals(OriginBinding.effectivePath))
                {
                    return compareInputBind;
                }
                return null;
            };

            foreach (InputActionBinder element in _inputActionBinders)
            {
                searchForDuplicatesOut = compareBinding(element, element.primary);
                if (searchForDuplicatesOut != null) return true;

                searchForDuplicatesOut = compareBinding(element, element.secondary);
                if (searchForDuplicatesOut != null) return true;

                searchForDuplicatesOut = compareBinding(element, element.controller);
                if (searchForDuplicatesOut != null) return true;
            }
            searchForDuplicatesOut = null;
            return false;
        }
    }
}