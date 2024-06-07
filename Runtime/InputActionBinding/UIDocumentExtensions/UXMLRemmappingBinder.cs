using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UtilEssentials.InputIconBinding;
using UtilEssentials.InputIconBinding.VisualElements;
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
        public InputActionBinder[] _inputActionBinders;


        [Space(10)]
        [Header("Input Actions")]
        [SerializeField] InputActionReference _resetBindingActionReference;
        public InputActionReference resetBindingActionReference => _resetBindingActionReference;

        [SerializeField] InputActionReference _emptyBindingActionReference;
        public InputActionReference emptyBindingActionReference => _emptyBindingActionReference;

        [SerializeField] InputActionReference _exitBindingPromptActionReference;
        public InputActionReference exitBindingPromptActionReference => _exitBindingPromptActionReference;

        [Space(10)]
        [Header("Icon Binding Info")]
        [SerializeField] string[] _tags;
        public string[] tags => _tags;

        [SerializeField] InputBindingSearchType _inputBindingSearchType;
        public InputBindingSearchType inputBindingSearchType => _inputBindingSearchType;

        [Serializable]
        public class InteractiveUXMLRebindEvent : UnityEvent<UXMLRemmappingBinder, InputActionRebindingExtensions.RebindingOperation>
        {
        }

        [Space(10)]
        [Header("Rebind Events")]
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
        internal bool localRebind
        {
            get => _localRebind;
            set { _localRebind = value; }
        }

        bool _doingRebinding;
        internal bool doingRebinding
        {
            get => _doingRebinding;
            set { _doingRebinding = value; }
        }

        UIDocument _UIDocument;
        UIDocumentExtender _UIDocumentExtender;

        internal UIDocumentExtender UIDocumentExtender => _UIDocumentExtender;

        public VisualElement focusedElement => _UIDocumentExtender.currentlyFocused;

        VisualElement _bindingOverlay;
        InputBindingIconElement _emptyBindingIcon;
        InputBindingIconElement _resetBindingIcon;

        Label _topLabel;
        InputBindingIconElement _bottomLabelIcon;

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

            _bindingOverlay = _UIDocument.rootVisualElement.Q("BindingOverlay");
            if (_bindingOverlay == null)
            {
                Debug.LogError($"UI Document {_UIDocument} does not contain element with name 'BindingOverlay'", this);
            }
            _topLabel = _bindingOverlay.Q<Label>("TopLabel");
            _bottomLabelIcon = _bindingOverlay.Q<InputBindingIconElement>("BottomLabelIcon");

            _emptyBindingIcon = _UIDocument.rootVisualElement.Q<InputBindingIconElement>("EmptyBindIcon");
            _resetBindingIcon = _UIDocument.rootVisualElement.Q<InputBindingIconElement>("ResetBindIcon");

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
        /// When the control method is switched to keyboard & mouse
        /// </summary>
        void UsingKeyboard()
        {
            foreach (var element in _inputActionBinders)
            {
                element.ToggleKeyboardBinding(true);
            }


            _resetBindingIcon.bindingCategory = InputBindingCategories.Keyboard;
            _emptyBindingIcon.bindingCategory = InputBindingCategories.Keyboard;

            var resetBindings = _resetBindingActionReference.action.bindings;
            for (int i = 0; i < resetBindings.Count; i++)
            {
                if (resetBindings[i].effectivePath.Contains("<Keyboard>") || resetBindings[i].effectivePath.Contains("<Mouse>"))
                {
                    _resetBindingIcon.inputBindingPath = resetBindings[i].effectivePath;
                    _resetBindingIcon.defaultText = _resetBindingActionReference.action.GetBindingDisplayString(i, InputBinding.DisplayStringOptions.DontIncludeInteractions);
                    _resetBindingIcon.UpdateIcon();
                }
            }

            var emptyBindings = _emptyBindingActionReference.action.bindings;
            for (int i = 0; i < emptyBindings.Count; i++)
            {
                if (emptyBindings[i].effectivePath.Contains("<Keyboard>") || emptyBindings[i].effectivePath.Contains("<Mouse>"))
                {
                    _emptyBindingIcon.inputBindingPath = emptyBindings[i].effectivePath;
                    _emptyBindingIcon.defaultText = _emptyBindingActionReference.action.GetBindingDisplayString(i, InputBinding.DisplayStringOptions.DontIncludeInteractions);
                    _emptyBindingIcon.UpdateIcon();
                }
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

            _resetBindingIcon.bindingCategory = InputBindingCategories.Gamepad;
            _emptyBindingIcon.bindingCategory = InputBindingCategories.Gamepad;

            var resetBindings = _resetBindingActionReference.action.bindings;
            for (int i = 0; i < resetBindings.Count; i++)
            {
                if (resetBindings[i].effectivePath.Contains("<Gamepad>"))
                {
                    _resetBindingIcon.inputBindingPath = resetBindings[i].effectivePath;
                    _resetBindingIcon.defaultText = _resetBindingActionReference.action.GetBindingDisplayString(i, InputBinding.DisplayStringOptions.DontIncludeInteractions);
                    _resetBindingIcon.UpdateIcon();
                }
            }

            var emptyBindings = _emptyBindingActionReference.action.bindings;
            for (int i = 0; i < emptyBindings.Count; i++)
            {
                if (resetBindings[i].effectivePath.Contains("<Gamepad>"))
                {
                    _emptyBindingIcon.inputBindingPath = emptyBindings[i].effectivePath;
                    _emptyBindingIcon.defaultText = _emptyBindingActionReference.action.GetBindingDisplayString(i, InputBinding.DisplayStringOptions.DontIncludeInteractions);
                    _emptyBindingIcon.UpdateIcon();
                }
            }
        }

        internal void ReplaceHoverTarget(InputBind inputBind)
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
        public void EnableOverlay(string bindType, string actionName, string path, InputBindingCategories category, string exitBinding)
        {
            _bindingOverlay.RemoveFromClassList("hidden");
            _topLabel.text = $"Rebinding {bindType} input of '{actionName}'";

            if (exitBinding != null)
            {
                _bottomLabelIcon.inputBindingPath = path;
                _bottomLabelIcon.bindingCategory = category;
                _bottomLabelIcon.defaultText = exitBinding;
                _bottomLabelIcon.UpdateIcon();
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
        internal bool SearchForDuplicates(InputBind OriginRemap, out InputBind searchForDuplicatesOut)
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