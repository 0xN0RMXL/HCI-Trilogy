using UnityEngine;
using UnityEngine.Audio;

namespace HCITrilogy.Core
{
    /// <summary>
    /// Base settings manager with common volume/mixer/PlayerPrefs logic.
    /// Subclasses add project-specific settings (VR locomotion, rhythm
    /// note speed, FPS sensitivity, etc.).
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField] protected AudioMixer mixer;

        public float MasterVolume { get; protected set; } = 0.9f;
        public float SfxVolume    { get; protected set; } = 0.9f;
        public float MusicVolume  { get; protected set; } = 0.6f;

        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
            LoadBase();
            ApplyMixer();
        }

        protected virtual void OnDestroy() => ServiceLocator.Unregister<SettingsManager>();

        public void SetMasterVolume(float v) { MasterVolume = Mathf.Clamp01(v); ApplyMixerParam("hci.vol.master", MasterVolume, "MasterVolume"); }
        public void SetSfxVolume(float v)    { SfxVolume    = Mathf.Clamp01(v); ApplyMixerParam("hci.vol.sfx",    SfxVolume,    "SFXVolume"); }
        public void SetMusicVolume(float v)  { MusicVolume  = Mathf.Clamp01(v); ApplyMixerParam("hci.vol.music",  MusicVolume,  "MusicVolume"); }

        protected void LoadBase()
        {
            MasterVolume = PlayerPrefs.GetFloat("hci.vol.master", 0.9f);
            SfxVolume    = PlayerPrefs.GetFloat("hci.vol.sfx",    0.9f);
            MusicVolume  = PlayerPrefs.GetFloat("hci.vol.music",  0.6f);
        }

        protected void ApplyMixer()
        {
            ApplyMixerParam("hci.vol.master", MasterVolume, "MasterVolume");
            ApplyMixerParam("hci.vol.sfx",    SfxVolume,    "SFXVolume");
            ApplyMixerParam("hci.vol.music",  MusicVolume,  "MusicVolume");
        }

        protected void ApplyMixerParam(string prefKey, float v01, string exposedParam)
        {
            PlayerPrefs.SetFloat(prefKey, v01);
            if (mixer == null) return;
            float db = v01 <= 0.0001f ? -80f : Mathf.Log10(v01) * 20f;
            mixer.SetFloat(exposedParam, db);
        }

        protected static float DbOf(float v01) =>
            v01 <= 0.0001f ? -80f : Mathf.Log10(v01) * 20f;
    }
}
