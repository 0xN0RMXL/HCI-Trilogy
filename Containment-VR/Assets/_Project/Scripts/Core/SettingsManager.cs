using UnityEngine;
using UnityEngine.Audio;

namespace HCITrilogy.Containment.Core
{
    /// <summary>
    /// VR-specific settings. Locomotion (teleport vs smooth), comfort (vignette
    /// on/off + intensity), snap-turn vs continuous-turn, dominant hand.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField] private AudioMixer mixer;
        public const string KeyMaster   = "hci.containment.vol.master";
        public const string KeySfx      = "hci.containment.vol.sfx";
        public const string KeyMusic    = "hci.containment.vol.music";
        public const string KeyLocomotion = "hci.containment.locomotion"; // 0=teleport, 1=smooth
        public const string KeySnapTurn   = "hci.containment.snapturn";   // 0=smooth, 1=snap
        public const string KeyVignette   = "hci.containment.vignette";   // 0=off, 1=on
        public const string KeyHandedness = "hci.containment.dominant";   // 0=right, 1=left

        public float MasterVolume { get; private set; } = 0.9f;
        public float SfxVolume    { get; private set; } = 0.9f;
        public float MusicVolume  { get; private set; } = 0.6f;
        public int   LocomotionMode { get; private set; } // 0 teleport, 1 smooth
        public int   TurnMode { get; private set; } = 1;  // 0 smooth, 1 snap
        public bool  Vignette { get; private set; } = true;
        public int   Dominant { get; private set; }       // 0 right, 1 left

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
            Load();
            ApplyMixer();
        }
        private void OnDestroy() => ServiceLocator.Unregister<SettingsManager>();

        public void SetMasterVolume(float v) { MasterVolume = Mathf.Clamp01(v); PlayerPrefs.SetFloat(KeyMaster, MasterVolume); ApplyMixer(); }
        public void SetSfxVolume(float v)    { SfxVolume    = Mathf.Clamp01(v); PlayerPrefs.SetFloat(KeySfx,    SfxVolume);    ApplyMixer(); }
        public void SetMusicVolume(float v)  { MusicVolume  = Mathf.Clamp01(v); PlayerPrefs.SetFloat(KeyMusic,  MusicVolume);  ApplyMixer(); }
        public void SetLocomotion(int v) { LocomotionMode = Mathf.Clamp(v, 0, 1); PlayerPrefs.SetInt(KeyLocomotion, LocomotionMode); }
        public void SetTurnMode(int v)   { TurnMode = Mathf.Clamp(v, 0, 1);       PlayerPrefs.SetInt(KeySnapTurn,   TurnMode); }
        public void SetVignette(bool v)  { Vignette = v;                          PlayerPrefs.SetInt(KeyVignette,   v ? 1 : 0); }
        public void SetDominant(int v)   { Dominant = Mathf.Clamp(v, 0, 1);       PlayerPrefs.SetInt(KeyHandedness, Dominant); }

        private void Load()
        {
            MasterVolume   = PlayerPrefs.GetFloat(KeyMaster, 0.9f);
            SfxVolume      = PlayerPrefs.GetFloat(KeySfx,    0.9f);
            MusicVolume    = PlayerPrefs.GetFloat(KeyMusic,  0.6f);
            LocomotionMode = PlayerPrefs.GetInt(KeyLocomotion, 0);
            TurnMode       = PlayerPrefs.GetInt(KeySnapTurn,   1);
            Vignette       = PlayerPrefs.GetInt(KeyVignette,   1) == 1;
            Dominant       = PlayerPrefs.GetInt(KeyHandedness, 0);
        }

        private void ApplyMixer()
        {
            if (mixer == null) return;
            mixer.SetFloat("MasterVolume", DbOf(MasterVolume));
            mixer.SetFloat("SFXVolume",    DbOf(SfxVolume));
            mixer.SetFloat("MusicVolume",  DbOf(MusicVolume));
        }

        private static float DbOf(float v01) => v01 <= 0.0001f ? -80f : Mathf.Log10(v01) * 20f;
    }
}
