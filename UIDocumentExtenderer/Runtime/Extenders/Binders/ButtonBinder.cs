using UnityEngine;
using UnityEngine.UIElements;

namespace UtilEssentials.UIDocumentExtenderer
{
    [RequireComponent(typeof(UIDocument))]
    public class ButtonBinder : MonoBehaviour
    {
        [SerializeField] ButtonBindings[] _buttons;
        UIDocument _UIDocument;
        UIDocumentExtender _UIDocumentExtender;


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

        void Loaded()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                var button = _UIDocument.rootVisualElement.Q(_buttons[i].Name) as Button;
                if (button == null)
                {
                    Debug.LogWarning($"There is no button with name {_buttons[i].Name} in UIDocument {_UIDocument}");
                    return;
                }
                int i2 = i;
                button.RegisterCallback<ClickEvent>((x) => _buttons[i2].ClickEvents.Invoke());
            }
        }

    }
}