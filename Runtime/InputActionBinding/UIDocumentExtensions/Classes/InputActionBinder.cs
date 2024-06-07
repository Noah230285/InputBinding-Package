using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace UtilEssentials.InputActionBinding.UIDocumentExtenderer
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
            if (InputRemapper == null)
            {
                Debug.LogWarning($"No Visual Element with name {_inputName} could be found");
                return;
            }
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
}