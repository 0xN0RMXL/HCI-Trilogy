using UnityEngine;

namespace HCITrilogy.Signal.Core
{
    /// <summary>
    /// Rhythm-game settings. Inherits shared volume/mixer logic from the core
    /// package and adds audio offset, accessibility, and note speed options.
    /// </summary>
    public class SettingsManager : HCITrilogy.Core.SettingsManager
    {
        public const string KeyAudioOffset  = "hci.signal.offset.ms";
        public const string KeyColorblind   = "hci.signal.accessibility.colorblind";
        public const string KeyHighContrast = "hci.signal.accessibility.highcontrast";
        public const string KeyHoldToPlay   = "hci.signal.accessibility.holdtoplay";
        public const string KeyNoteSpeed    = "hci.signal.notespeed";

        public int   AudioOffsetMs { get; private set; }
        public bool  Colorblind    { get; private set; }
        public bool  HighContrast  { get; private set; }
        public bool  HoldToPlay    { get; private set; }
        public float NoteSpeed     { get; private set; } = 6f;

        protected override void Awake()
        {
            base.Awake();
            LoadRhythm();
        }

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

        private void LoadRhythm()
        {
            AudioOffsetMs = PlayerPrefs.GetInt(KeyAudioOffset, 0);
            Colorblind    = PlayerPrefs.GetInt(KeyColorblind, 0) == 1;
            HighContrast  = PlayerPrefs.GetInt(KeyHighContrast, 0) == 1;
            HoldToPlay    = PlayerPrefs.GetInt(KeyHoldToPlay, 0) == 1;
            NoteSpeed     = PlayerPrefs.GetFloat(KeyNoteSpeed, 6f);
        }
    }
}
