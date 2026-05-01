using UnityEngine;
using UnityEngine.UI;
using HCITrilogy.Lockdown.Core;

namespace HCITrilogy.Lockdown.UI
{
    /// <summary>
    /// Wires the settings panel sliders/toggles to SettingsManager.
    /// Reads current values on enable; writes on change.
    /// </summary>
    public class SettingsPanelController : MonoBehaviour
    {
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Toggle invertYToggle;
        [SerializeField] private Toggle headBobToggle;
        [SerializeField] private Button closeButton;

        private SettingsManager _s;

        private void OnEnable()
        {
            _s = ServiceLocator.Get<SettingsManager>();
            if (_s == null) return;

            if (masterSlider)      { masterSlider.value = _s.MasterVolume; masterSlider.onValueChanged.AddListener(_s.SetMasterVolume); }
            if (sfxSlider)         { sfxSlider.value = _s.SfxVolume; sfxSlider.onValueChanged.AddListener(_s.SetSfxVolume); }
            if (musicSlider)       { musicSlider.value = _s.MusicVolume; musicSlider.onValueChanged.AddListener(_s.SetMusicVolume); }
            if (sensitivitySlider) { sensitivitySlider.value = _s.SensitivityY / 4f; sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged); }
            if (invertYToggle)     { invertYToggle.isOn = _s.InvertY; invertYToggle.onValueChanged.AddListener(_s.SetInvertY); }
            if (headBobToggle)     { headBobToggle.isOn = _s.HeadBob; headBobToggle.onValueChanged.AddListener(_s.SetHeadBob); }
            if (closeButton)       closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void OnSensitivityChanged(float v01)
        {
            float mapped = Mathf.Lerp(0.1f, 4f, v01);
            _s.SetSensitivityX(mapped);
            _s.SetSensitivityY(mapped);
        }
    }
}
