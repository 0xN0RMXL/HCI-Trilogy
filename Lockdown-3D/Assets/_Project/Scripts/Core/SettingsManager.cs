using UnityEngine;
using UnityEngine.Audio;

namespace HCITrilogy.Lockdown.Core
{
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField] private AudioMixer mixer;
        public const string KeyMaster   = "hci.lockdown.vol.master";
        public const string KeySfx      = "hci.lockdown.vol.sfx";
        public const string KeyMusic    = "hci.lockdown.vol.music";
        public const string KeySensX    = "hci.lockdown.sensitivity.x";
        public const string KeySensY    = "hci.lockdown.sensitivity.y";
        public const string KeyInvertY  = "hci.lockdown.invert.y";
        public const string KeyHeadbob  = "hci.lockdown.headbob";

        public float MasterVolume { get; private set; } = 0.9f;
        public float SfxVolume    { get; private set; } = 0.9f;
        public float MusicVolume  { get; private set; } = 0.6f;
        public float SensitivityX { get; private set; } = 1.0f;
        public float SensitivityY { get; private set; } = 1.0f;
        public bool  InvertY      { get; private set; }
        public bool  HeadBob      { get; private set; } = true;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
            Load();
            ApplyMixer();
        }

        private void OnDestroy() => ServiceLocator.Unregister<SettingsManager>();

        public void SetMasterVolume(float v) { MasterVolume = Mathf.Clamp01(v); Save(KeyMaster, MasterVolume); ApplyMixer(); }
        public void SetSfxVolume(float v)    { SfxVolume    = Mathf.Clamp01(v); Save(KeySfx,    SfxVolume);    ApplyMixer(); }
        public void SetMusicVolume(float v)  { MusicVolume  = Mathf.Clamp01(v); Save(KeyMusic,  MusicVolume);  ApplyMixer(); }
        public void SetSensitivityX(float v) { SensitivityX = Mathf.Clamp(v, 0.1f, 4f); Save(KeySensX, SensitivityX); }
        public void SetSensitivityY(float v) { SensitivityY = Mathf.Clamp(v, 0.1f, 4f); Save(KeySensY, SensitivityY); }
        public void SetInvertY(bool v)       { InvertY = v; PlayerPrefs.SetInt(KeyInvertY, v ? 1 : 0); }
        public void SetHeadBob(bool v)       { HeadBob = v; PlayerPrefs.SetInt(KeyHeadbob, v ? 1 : 0); }

        private static void Save(string key, float v) => PlayerPrefs.SetFloat(key, v);

        private void Load()
        {
            MasterVolume  = PlayerPrefs.GetFloat(KeyMaster, 0.9f);
            SfxVolume     = PlayerPrefs.GetFloat(KeySfx, 0.9f);
            MusicVolume   = PlayerPrefs.GetFloat(KeyMusic, 0.6f);
            SensitivityX  = PlayerPrefs.GetFloat(KeySensX, 1.0f);
            SensitivityY  = PlayerPrefs.GetFloat(KeySensY, 1.0f);
            InvertY       = PlayerPrefs.GetInt(KeyInvertY, 0) == 1;
            HeadBob       = PlayerPrefs.GetInt(KeyHeadbob, 1) == 1;
        }

        private void ApplyMixer()
        {
            if (mixer == null) return;
            mixer.SetFloat("MasterVolume", DbOf(MasterVolume));
            mixer.SetFloat("SFXVolume",    DbOf(SfxVolume));
            mixer.SetFloat("MusicVolume",  DbOf(MusicVolume));
        }

        private static float DbOf(float v01) =>
            v01 <= 0.0001f ? -80f : Mathf.Log10(v01) * 20f;
    }
}
