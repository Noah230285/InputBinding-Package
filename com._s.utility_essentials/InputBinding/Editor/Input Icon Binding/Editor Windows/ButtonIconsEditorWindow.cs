using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace UtilEssentials.InputActionBinding.Editor
{
    class ButtonIconsEditorWindow : EditorWindow
    {
        ButtonIconBindingsElement _bindingsElement;

        SOButtonIconBindings _activeBindingSO;
        SerializedObject _activeBindingSOSerializedObject;


        [MenuItem("Window/Button Icon Binding...")]
        public static void OpenWindow()
        {
            OpenWindow(null);
        }

        public static void OpenWindow(SOButtonIconBindings bindingSO)
        {
            ButtonIconsEditorWindow wnd = GetWindow<ButtonIconsEditorWindow>();
            wnd.titleContent = new GUIContent("Button Icon Bindings");
            wnd.SelectionIsBindingSO(bindingSO);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            SOButtonIconBindings bindingSO = Selection.activeObject as SOButtonIconBindings;
            if (bindingSO != null)
            {
                OpenWindow(bindingSO);
                return true;
            }
            return false;
        }

        public void CreateGUI()
        {
            _activeBindingSO = null;
            _bindingsElement = new ButtonIconBindingsElement();
            rootVisualElement.Add(_bindingsElement);
            OnSelectionChange();
            _bindingsElement.BindSOButtonIconBindings(_activeBindingSOSerializedObject);
        }

        private void OnSelectionChange()
        {
            SOButtonIconBindings bindingSO = Selection.activeObject as SOButtonIconBindings;
            if (bindingSO == null)
            {
                return;
            }

            SelectionIsBindingSO(bindingSO);
        }

        void SelectionIsBindingSO(SOButtonIconBindings bindingSO)
        {
            if (_activeBindingSO == bindingSO)
            {
                return;
            }

            _activeBindingSO = bindingSO;
            _activeBindingSOSerializedObject = new SerializedObject(bindingSO);
            _bindingsElement.BindSOButtonIconBindings(_activeBindingSOSerializedObject);
        }
    }
}