using UnityEngine;

namespace UtilEssentials.ScriptableVariables
{
    [CreateAssetMenu(menuName = "Util Essentials/Scriptable Variables/Misc/Prefab Container")]
    public class PrefabContainer : ScriptableObject
    {
        public GameObject[] Prefabs;
    }

}
