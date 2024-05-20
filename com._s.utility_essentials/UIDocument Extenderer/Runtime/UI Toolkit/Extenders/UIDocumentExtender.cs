using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace UtilEssentials.UIDocumentExtenderer
{
    [RequireComponent(typeof(UIDocument)), DefaultExecutionOrder(1)]
    public class UIDocumentExtender : MonoBehaviour
    {
        UIDocument _document;
        string _controlScheme;
        public string controlScheme => _controlScheme;

        VisualElement _currentlyFocused;
        public VisualElement currentlyFocused => _currentlyFocused;

        VisualElement _previouslyFocused;
        public VisualElement previouslyFocused => _previouslyFocused;


        [SerializeField] PlayerInput _UIInput;
        public PlayerInput UIInput => _UIInput;

        public event Action UsingKeyboard;
        public event Action UsingGamepad;
        public event Action _UILoaded;

        bool _isLoaded;
        public bool isLoaded => _isLoaded;
        void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        void OnEnable()
        {
            _isLoaded = false;
            if (UILoader.instance.isLoading)
            {
                UILoader.instance.LoadComplete += OnUILoaded;
                return;
            }
            OnUILoaded();
        }

        void OnUILoaded()
        {
            UILoader.instance.LoadComplete -= OnUILoaded;
            _isLoaded = true;

            _document.rootVisualElement.RegisterCallback<FocusInEvent>(evt =>
            {
                _currentlyFocused = evt.target as VisualElement;
            });

            _document.rootVisualElement.RegisterCallback<FocusOutEvent>(evt =>
            {
                var element = evt.target as VisualElement;
                _previouslyFocused = element;
                StartCoroutine(DelayCheck());
                IEnumerator DelayCheck()
                {
                    yield return null;
                    if (_currentlyFocused == element)
                    {
                        _previouslyFocused = null;
                        element.Focus();
                    }
                }
            });

            InputSystem.onActionChange += CheckControlSchemeName;
            StartCoroutine(DelayControlSchemeChange());
            IEnumerator DelayControlSchemeChange()
            {
                yield return new WaitForEndOfFrame();
                CheckControlSchemeName(null, new InputActionChange());
            }

            _UILoaded?.Invoke();
        }

        private void OnDisable()
        {
            InputSystem.onActionChange -= CheckControlSchemeName;
            UILoader.instance.LoadComplete -= OnUILoaded;

            _isLoaded = false;
            _currentlyFocused = null;
            _previouslyFocused = null;
        }

        bool IterateElements(VisualElement element, Func<VisualElement, bool> iterateAction)
        {
            for (int i = 0; i < element.childCount; i++)
            {
                var child = element.ElementAt(i);
                if (iterateAction(child))
                {
                    return true;
                }
                if (IterateElements(child, iterateAction))
                {
                    return true;
                }
            }
            return false;
        }

        void CheckControlSchemeName(object obj, InputActionChange change)
        {
            if (_controlScheme == UIInput.currentControlScheme)
            {
                return;
            }
            _controlScheme = UIInput.currentControlScheme;
            foreach (var scheme in UIInput.actions.controlSchemes)
            {
                if (scheme.name.Equals(_controlScheme))
                {
                    foreach (var requirement in scheme.deviceRequirements)
                    {
                        switch (requirement.controlPath)
                        {
                            case "<Keyboard>":
                                SwitchedToMouseAndKeyboard();
                                return;
                            case "<Gamepad>":
                                SwitchedToGamepad();
                                return;
                            default:
                                break;
                        }
                    }
                    Debug.LogWarning($"No control path for control scheme {scheme.deviceRequirements} in {UIInput.actions} setup", this);
                    return;
                }
            }
        }

        void SwitchedToGamepad()
        {
            UsingGamepad?.Invoke();

            if (_currentlyFocused != null)
            {
                return;
            }
            if (_previouslyFocused != null && _previouslyFocused.focusable)
            {
                _previouslyFocused.Focus();
                return;
            }

            IterateElements(_document.rootVisualElement, (VisualElement element) =>
            {
                if (element.canGrabFocus)
                {
                    element.Focus();
                    return true;
                }
                return false;
            });
        }

        void SwitchedToMouseAndKeyboard()
        {
            UsingKeyboard?.Invoke();
        }
    }

}
