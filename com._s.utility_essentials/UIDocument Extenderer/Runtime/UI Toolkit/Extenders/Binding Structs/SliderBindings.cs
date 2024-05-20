using System;
using UnityEngine.Audio;
using UnityEngine;
using UtilEssentials.ScriptableVariables;
using UtilEssentials.UIToolkitUtility.Attributes;

namespace UtilEssentials.UIDocumentExtenderer
{
    [Serializable]
    public struct SliderBindings
    {
        public string Name;
        public enum SliderBindType
        {
            ClampedFloat,
            AudioMixerVolume
        }

        [Serializable]
        public struct AudioMixerValues
        {
            public AudioMixer Mixer;
            public string ValueName;
        }

        [EnumBinding(new string[] { "BindClampedFloat", "BindAudioMixer" })]
        public SliderBindType BindType;
        [HideInInspector] public ClampedFloatReference BindClampedFloat;
        [HideInInspector] public AudioMixerValues BindAudioMixer;
    }
}