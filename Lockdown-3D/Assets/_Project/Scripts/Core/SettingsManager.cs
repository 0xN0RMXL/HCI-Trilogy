using UnityEngine;

namespace HCITrilogy.Lockdown.Core
{
    /// <summary>
    /// FPS-specific settings. Inherits shared volume/mixer logic from the core
    /// package and adds sensitivity, invert-Y, and head-bob options.
    /// </summary>
    public class SettingsManager : HCITrilogy.Core.SettingsManager
    {
        public const string KeySensX   = "hci.lockdown.sensitivity.x";
        public const string KeySensY   = "hci.lockdown.sensitivity.y";
        public const string KeyInvertY = "hci.lockdown.invert.y";
        public const string KeyHeadbob = "hci.lockdown.headbob";

        public float SensitivityX { get; private set; } = 1.0f;
        public float SensitivityY { get; private set; } = 1.0f;
        public bool  InvertY      { get; private set; }
        public bool  HeadBob      { get; private set; } = true;

        protected override void Awake()
        {
            base.Awake();
            LoadFPS();
        }

        public void SetSensitivityX(float v) { SensitivityX = Mathf.Clamp(v, 0.1f, 4f); PlayerPrefs.SetFloat(KeySensX, SensitivityX); }
        public void SetSensitivityY(float v) { SensitivityY = Mathf.Clamp(v, 0.1f, 4f); PlayerPrefs.SetFloat(KeySensY, SensitivityY); }
        public void SetInvertY(bool v)       { InvertY = v; PlayerPrefs.SetInt(KeyInvertY, v ? 1 : 0); }
        public void SetHeadBob(bool v)       { HeadBob = v; PlayerPrefs.SetInt(KeyHeadbob, v ? 1 : 0); }

        private void LoadFPS()
        {
            SensitivityX = PlayerPrefs.GetFloat(KeySensX, 1.0f);
            SensitivityY = PlayerPrefs.GetFloat(KeySensY, 1.0f);
            InvertY      = PlayerPrefs.GetInt(KeyInvertY, 0) == 1;
            HeadBob      = PlayerPrefs.GetInt(KeyHeadbob, 1) == 1;
        }
    }
}
