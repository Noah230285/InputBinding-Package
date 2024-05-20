using UnityEngine;
using UnityEngine.UIElements;

namespace UtilEssentials.UIDocumentExtenderer
{
    [RequireComponent(typeof(UIDocument))]
    public class ButtonBinder : MonoBehaviour
    {
        [SerializeField] ButtonBindings[] _buttons;
        UIDocument _UIDocument;

        void Awake()
        {
            TryGetComponent(out _UIDocument);
        }

        void OnEnable()
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

        private void OnDisable()
        {

        }
    }
}