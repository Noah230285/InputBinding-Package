using UnityEngine;
using UnityEngine.UIElements;

namespace UtilEssentials.UIDocumentExtenderer
{
    [RequireComponent(typeof(UIDocument))]
    public class SliderBinder : MonoBehaviour
    {
        [SerializeField] SliderBindings[] _sliders;
        UIDocument _UIDocument;

        void Awake()
        {
            TryGetComponent(out _UIDocument);
        }

        void OnEnable()
        {
            for (int i = 0; i < _sliders.Length; i++)
            {
                var root = _UIDocument.rootVisualElement;
                var sliderContainer = root.Q(_sliders[i].Name).Q<Slider>();
                if (sliderContainer == null)
                {
                    Debug.LogWarning($"There is no slider container with name {_sliders[i].Name} in UIDocument {_UIDocument}");
                    return;
                }
                var slider = sliderContainer.Q<Slider>();
                if (slider == null)
                {
                    Debug.LogWarning($"There is no slider field within {sliderContainer} in UIDocument {_UIDocument}");
                    return;
                }
                var m_bar = new VisualElement();
                m_bar.AddToClassList("slider-fill");
                if (slider.Q<VisualElement>("unity-dragger").childCount == 0)
                {
                    slider.Q<VisualElement>("unity-dragger").Add(m_bar);

                    var m_drag = new VisualElement();
                    m_drag.AddToClassList("slider-draggable");
                    slider.Q<VisualElement>("unity-dragger").Add(m_drag);
                }

                Slider sliderDummy = slider;
                int i2 = i;
                switch (_sliders[i2].BindType)
                {
                    case SliderBindings.SliderBindType.ClampedFloat:
                        sliderDummy.value = _sliders[i2].BindClampedFloat.current;
                        sliderDummy.highValue = _sliders[i2].BindClampedFloat.max;
                        sliderDummy.RegisterValueChangedCallback((x) =>
                        {
                            if (slider != null)
                            {
                                _sliders[i2].BindClampedFloat.current = slider.value;
                            }
                        });
                        break;
                    case SliderBindings.SliderBindType.AudioMixerVolume:
                        _sliders[i2].BindAudioMixer.Mixer.GetFloat(_sliders[i2].BindAudioMixer.ValueName, out float current);
                        sliderDummy.lowValue = 0.0001f;
                        sliderDummy.value = Mathf.Pow(10, current / 20);
                        sliderDummy.highValue = 1;
                        sliderDummy.RegisterValueChangedCallback((x) =>
                        {
                            if (slider != null)
                            {
                                _sliders[i2].BindAudioMixer.Mixer.SetFloat(_sliders[i2].BindAudioMixer.ValueName, Mathf.Log10(slider.value) * 20f);
                            }
                        });

                        break;
                    default:
                        break;
                }
            }
        }
    }
}