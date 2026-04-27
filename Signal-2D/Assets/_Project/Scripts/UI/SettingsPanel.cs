using UnityEngine;
using UnityEngine.UI;
using HCITrilogy.Signal.Core;

namespace HCITrilogy.Signal.UI
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private Toggle colorblindToggle;
        [SerializeField] private Toggle highContrastToggle;
        [SerializeField] private Toggle holdToPlayToggle;
        [SerializeField] private Slider noteSpeedSlider;
        [SerializeField] private InputField audioOffsetField;
        [SerializeField] private Button   recalibrateButton;
        [SerializeField] private Button   closeButton;

        private SettingsManager _s;

        private void OnEnable()
        {
            _s = ServiceLocator.Get<SettingsManager>();
            if (_s == null) return;
            if (masterSlider) masterSlider.value = _s.MasterVolume;
            if (sfxSlider)    sfxSlider.value    = _s.SfxVolume;
            if (musicSlider)  musicSlider.value  = _s.MusicVolume;
            if (vsyncToggle)  vsyncToggle.isOn   = QualitySettings.vSyncCount > 0;
            if (colorblindToggle)   colorblindToggle.isOn   = _s.Colorblind;
            if (highContrastToggle) highContrastToggle.isOn = _s.HighContrast;
            if (holdToPlayToggle)   holdToPlayToggle.isOn   = _s.HoldToPlay;
            if (noteSpeedSlider)    noteSpeedSlider.value   = _s.NoteSpeed;
            if (audioOffsetField)   audioOffsetField.text   = _s.AudioOffsetMs.ToString();

            if (masterSlider) masterSlider.onValueChanged.AddListener(_s.SetMasterVolume);
            if (sfxSlider)    sfxSlider.onValueChanged.AddListener(_s.SetSfxVolume);
            if (musicSlider)  musicSlider.onValueChanged.AddListener(_s.SetMusicVolume);
            if (vsyncToggle)  vsyncToggle.onValueChanged.AddListener(v => QualitySettings.vSyncCount = v ? 1 : 0);
            if (colorblindToggle)   colorblindToggle.onValueChanged.AddListener(_s.SetColorblind);
            if (highContrastToggle) highContrastToggle.onValueChanged.AddListener(_s.SetHighContrast);
            if (holdToPlayToggle)   holdToPlayToggle.onValueChanged.AddListener(_s.SetHoldToPlay);
            if (noteSpeedSlider)    noteSpeedSlider.onValueChanged.AddListener(_s.SetNoteSpeed);
            if (audioOffsetField)   audioOffsetField.onEndEdit.AddListener(s =>
                { if (int.TryParse(s, out var ms)) _s.SetAudioOffsetMs(ms); });
            if (recalibrateButton)  recalibrateButton.onClick.AddListener(() =>
                SceneFlow.Instance?.LoadAsync("Calibration"));
            if (closeButton)        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }
    }
}
