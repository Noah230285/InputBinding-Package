using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UtilEssentials.UIToolkitUtility.Editor;

public class AddIconBindingAddressables : MonoBehaviour
{
    [InitializeOnLoadMethod]
    static void Init()
    {
        //Find the path for this package
        string assetPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
        string beginningPath = UIToolkitUtilityFunctions.GetBeginningOfPackagePath(assetPath, "com.utility_essentials.input_binding");

        var group = AssetDatabase.LoadAssetAtPath<AddressableAssetGroup>($"{beginningPath}/Assets/ScriptableObjects/Addressables/UXML Menu Assets.asset");
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (!settings.groups.Contains(group))
        {
            settings.groups.Add(group);
        }
    }
}
