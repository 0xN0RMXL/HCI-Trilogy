using UnityEngine;
using UnityEngine.Audio;

namespace HCITrilogy.Signal.Core
{
    /// <summary>
    /// Owns master/SFX/music volume + accessibility settings.
    /// Persists via PlayerPrefs. Drives the shared AudioMixer.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField] private AudioMixer mixer;

        // PlayerPrefs keys
        public const string KeyMasterVol    = "hci.signal.vol.master";
        public const string KeySfxVol       = "hci.signal.vol.sfx";
        public const string KeyMusicVol     = "hci.signal.vol.music";
        public const string KeyAudioOffset  = "hci.signal.offset.ms";
        public const string KeyColorblind   = "hci.signal.accessibility.colorblind";
        public const string KeyHighContrast = "hci.signal.accessibility.highcontrast";
        public const string KeyHoldToPlay   = "hci.signal.accessibility.holdtoplay";
        public const string KeyNoteSpeed    = "hci.signal.notespeed";

        public float MasterVolume { get; private set; } = 0.9f;
        public float SfxVolume    { get; private set; } = 0.9f;
        public float MusicVolume  { get; private set; } = 0.7f;

        public int AudioOffsetMs  { get; private set; } = 0;
        public bool Colorblind    { get; private set; }
        public bool HighContrast  { get; private set; }
        public bool HoldToPlay    { get; private set; }
        public float NoteSpeed    { get; private set; } = 6f;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
            Load();
            ApplyAll();
        }

        private void OnDestroy() => ServiceLocator.Unregister<SettingsManager>();

        public void SetMasterVolume(float v01) { MasterVolume = Mathf.Clamp01(v01); ApplyMixer(KeyMasterVol, MasterVolume, "MasterVolume"); }
        public void SetSfxVolume(float v01)    { SfxVolume    = Mathf.Clamp01(v01); ApplyMixer(KeySfxVol,    SfxVolume,    "SFXVolume"); }
        public void SetMusicVolume(float v01)  { MusicVolume  = Mathf.Clamp01(v01); ApplyMixer(KeyMusicVol,  MusicVolume,  "MusicVolume"); }

        public void SetAudioOffsetMs(int ms)
        {
            AudioOffsetMs = Mathf.Clamp(ms, -300, 300);
            PlayerPrefs.SetInt(KeyAudioOffset, AudioOffsetMs);
        }

        public void SetColorblind(bool v)   { Colorblind = v; PlayerPrefs.SetInt(KeyColorblind, v ? 1 : 0); }
        public void SetHighContrast(bool v) { HighContrast = v; PlayerPrefs.SetInt(KeyHighContrast, v ? 1 : 0); }
        public void SetHoldToPlay(bool v)   { HoldToPlay = v; PlayerPrefs.SetInt(KeyHoldToPlay, v ? 1 : 0); }

        public void SetNoteSpeed(float speed)
        {
            NoteSpeed = Mathf.Clamp(speed, 3f, 12f);
            PlayerPrefs.SetFloat(KeyNoteSpeed, NoteSpeed);
        }

        private void Load()
        {
            MasterVolume  = PlayerPrefs.GetFloat(KeyMasterVol, 0.9f);
            SfxVolume     = PlayerPrefs.GetFloat(KeySfxVol, 0.9f);
            MusicVolume   = PlayerPrefs.GetFloat(KeyMusicVol, 0.7f);
            AudioOffsetMs = PlayerPrefs.GetInt(KeyAudioOffset, 0);
            Colorblind    = PlayerPrefs.GetInt(KeyColorblind, 0) == 1;
            HighContrast  = PlayerPrefs.GetInt(KeyHighContrast, 0) == 1;
            HoldToPlay    = PlayerPrefs.GetInt(KeyHoldToPlay, 0) == 1;
            NoteSpeed     = PlayerPrefs.GetFloat(KeyNoteSpeed, 6f);
        }

        private void ApplyAll()
        {
            ApplyMixer(KeyMasterVol, MasterVolume, "MasterVolume");
            ApplyMixer(KeySfxVol,    SfxVolume,    "SFXVolume");
            ApplyMixer(KeyMusicVol,  MusicVolume,  "MusicVolume");
        }

        private void ApplyMixer(string prefKey, float v01, string exposedParam)
        {
            PlayerPrefs.SetFloat(prefKey, v01);
            if (mixer == null) return;
            // Map [0.0001..1.0] to [-80..0] dB (log).
            float db = v01 <= 0.0001f ? -80f : Mathf.Log10(v01) * 20f;
            mixer.SetFloat(exposedParam, db);
        }
    }
}
