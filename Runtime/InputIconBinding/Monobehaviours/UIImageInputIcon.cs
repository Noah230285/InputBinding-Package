using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UtilEssentials.InputIconBinding;
using UtilEssentials.UIDocumentExtenderer;

namespace UtilEssentials.InputIconBinding
{
    public class UIImageInputIcon : MonoBehaviour
    {
        [Serializable]
        struct StaticReferenceBinding
        {
            [SerializeField] InputActionReference _actionReference;
            [SerializeField] int _bindingIndex;
            public int bindingIndex => _bindingIndex;

            [SerializeField] string _bindingID;
        }
        [SerializeField] StaticReferenceBinding _staticReferenceBinding;


        [SerializeField] InputActionReference _displayedActionInput;

        [SerializeField] Image _iconImage;
        [SerializeField, Tooltip("This text will be displayed if a valid icon cannot be found")]
        Text _iconBackupText;


        [Space(10)]
        [Header("Icon Binding Info")]
        [SerializeField] InputBindingSearchType _inputBindingSearchType;

        [SerializeField] string _nameID;
        [SerializeField] string[] _tags;

        Coroutine iconAnimationCoroutine;

        void OnEnable()
        {
            PlayerInputExtender.instance.InputsRebound += InputsRebound;

            if (_staticReferenceBinding.bindingIndex >= 0)
            {
                return;
            }
            PlayerInputExtender.instance.SwitchedToGamepad += SwitchedToGamepad;
            PlayerInputExtender.instance.SwitchedToKeyboardAndMouse += SwitchedToKeyboard;
            switch (PlayerInputExtender.instance.controlPath)
            {
                case "<Keyboard>":
                    SwitchedToKeyboard();
                    break;
                case "<Gamepad>":
                    SwitchedToGamepad();
                    break;
            }
        }

        void OnDisable()
        {
            PlayerInputExtender.instance.InputsRebound -= InputsRebound;

            if (_staticReferenceBinding.bindingIndex >= 0)
            {
                return;
            }
            PlayerInputExtender.instance.SwitchedToGamepad -= SwitchedToGamepad;
            PlayerInputExtender.instance.SwitchedToKeyboardAndMouse -= SwitchedToKeyboard;
        }

        void Start()
        {
            if (_staticReferenceBinding.bindingIndex >= 0)
            {
                SwitchIconFromBindIndex(_staticReferenceBinding.bindingIndex);
            }
        }

        void SwitchedToKeyboard()
        {
            SwitchedToControlMethod("Keyboard");
        }

        void SwitchedToGamepad()
        {
            SwitchedToControlMethod("Gamepad");
        }

        void InputsRebound(object obj, string currentControlPath)
        {
            if (_displayedActionInput == null)
            {
                return;
            }

            var action = obj as InputAction;
            var actionMap = action?.actionMap ?? obj as InputActionMap;
            var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

            var referencedAction = _displayedActionInput.action;

            if (referencedAction != action &&
                referencedAction.actionMap != actionMap &&
                referencedAction.actionMap?.asset != actionAsset)
            {
                return;
            }

            if (_staticReferenceBinding.bindingIndex >= 0)
            {
                SwitchIconFromBindIndex(_staticReferenceBinding.bindingIndex);
            }
            else
            {
                switch (currentControlPath)
                {
                    case "<Keyboard>":
                        SwitchedToKeyboard();
                        break;
                    case "<Gamepad>":
                        SwitchedToGamepad();
                        break;
                }
            }

        }

        void SwitchedToControlMethod(string controlMethod)
        {
            if (_displayedActionInput == null)
            {
                return;
            }

            int i = 0;
            bool bindingFound = false;
            var bindings = _displayedActionInput.action.bindings;
            InputBindingCategories ControlCategory = InputBindingCategories.Keyboard;

            for (; i < bindings.Count; i++)
            {
                bool isValid = false;
                switch (controlMethod)
                {
                    case "Keyboard":
                        if (bindings[i].effectivePath.Contains("<Keyboard>"))
                        {
                            isValid = true;
                            ControlCategory = InputBindingCategories.Keyboard;
                        }
                        else if (bindings[i].effectivePath.Contains("<Mouse>"))
                        {
                            isValid = true;
                            ControlCategory = InputBindingCategories.Mouse;
                        }
                        break;
                    case "Gamepad":
                        isValid = bindings[i].effectivePath.Contains("<Gamepad>");
                        ControlCategory = InputBindingCategories.Gamepad;
                        break;
                }

                if (isValid)
                {
                    bindingFound = true;
                    break;
                }
            }

            if (!bindingFound)
            {
                _iconImage.enabled = false;
                _iconBackupText.enabled = false;
                return;
            }
            SwitchIconFromBindIndex(i, ControlCategory);
        }

        void SwitchIconFromBindIndex(int bindIndex, InputBindingCategories category = (InputBindingCategories)(-1))
        {
            var bindings = _displayedActionInput.action.bindings;

            if (!ButtonIconBindingsEditorSettings.GetIconBindData(out IconBindData iconBindData, bindings[bindIndex].effectivePath, _nameID, category, _tags, _inputBindingSearchType))
            {
                _iconBackupText.enabled = true;
                _iconImage.enabled = false;

                _iconBackupText.text = _displayedActionInput.action.GetBindingDisplayString(bindIndex, InputBinding.DisplayStringOptions.DontIncludeInteractions);
                return;
            }

            _iconImage.enabled = true;
            _iconBackupText.enabled = false;

            if (iconAnimationCoroutine != null)
            {
                StopCoroutine(iconAnimationCoroutine);
            }

            switch (iconBindData.type)
            {
                case IconType.Static:

                    Texture2D tex = iconBindData.texture;
                    _iconImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                    break;

                case IconType.Animated_ReverseLoop:

                    iconAnimationCoroutine = StartCoroutine(IconAnimation_ReverseLoop(iconBindData));
                    break;

                case IconType.Animated_RolloverLoop:
                    iconAnimationCoroutine = StartCoroutine(IconAnimation_RolloverLoop(iconBindData));
                    break;
            }
        }

        IEnumerator IconAnimation_ReverseLoop(IconBindData iconBindData)
        {
            float timeBetweenFrames = iconBindData.animSprites.Length / iconBindData.animationTime;
            float currentFrameTime = 0;
            int frameIndex = 0;
            int direction = 1;
            _iconImage.sprite = iconBindData.animSprites[0];

            while (true)
            {
                if (currentFrameTime >= timeBetweenFrames)
                {
                    if ((frameIndex >= iconBindData.animSprites.Length - 1 && direction > 0) || (frameIndex <= 0 && direction < 0))
                    {
                        direction *= -1;
                    }
                    frameIndex += direction;

                    _iconImage.sprite = iconBindData.animSprites[frameIndex];
                }
                yield return null;
                currentFrameTime += Time.deltaTime;
            }
        }

        IEnumerator IconAnimation_RolloverLoop(IconBindData iconBindData)
        {
            float timeBetweenFrames = iconBindData.animSprites.Length / iconBindData.animationTime;
            float currentFrameTime = 0;
            int frameIndex = 0;
            _iconImage.sprite = iconBindData.animSprites[0];

            while (true)
            {
                if (currentFrameTime >= timeBetweenFrames)
                {
                    if (frameIndex >= iconBindData.animSprites.Length - 1)
                    {
                        frameIndex = 0;
                    }
                    frameIndex++;

                    _iconImage.sprite = iconBindData.animSprites[frameIndex];
                }
                yield return null;
                currentFrameTime += Time.deltaTime;
            }
        }
    }
}