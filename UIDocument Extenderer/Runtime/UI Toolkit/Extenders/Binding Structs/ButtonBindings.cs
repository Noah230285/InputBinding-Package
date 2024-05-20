using System;
using UnityEngine.Events;

namespace UtilEssentials.UIDocumentExtenderer
{
    [Serializable]
    public struct ButtonBindings
    {
        public string Name;
        public UnityEvent ClickEvents;
    }
}