using UnityEngine;

namespace HCITrilogy.Containment.Core
{
    /// <summary>
    /// VR-specific settings. Inherits shared volume/mixer logic from the core
    /// package and adds locomotion, comfort, and handedness options.
    /// </summary>
    public class SettingsManager : HCITrilogy.Core.SettingsManager
    {
        public const string KeyLocomotion = "hci.containment.locomotion";
        public const string KeySnapTurn   = "hci.containment.snapturn";
        public const string KeyVignette   = "hci.containment.vignette";
        public const string KeyHandedness = "hci.containment.dominant";

        public int  LocomotionMode { get; private set; }
        public int  TurnMode       { get; private set; } = 1;
        public bool Vignette       { get; private set; } = true;
        public int  Dominant       { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            LoadVR();
        }

        public void SetLocomotion(int v) { LocomotionMode = Mathf.Clamp(v, 0, 1); PlayerPrefs.SetInt(KeyLocomotion, LocomotionMode); }
        public void SetTurnMode(int v)   { TurnMode = Mathf.Clamp(v, 0, 1);       PlayerPrefs.SetInt(KeySnapTurn,   TurnMode); }
        public void SetVignette(bool v)  { Vignette = v;                          PlayerPrefs.SetInt(KeyVignette,   v ? 1 : 0); }
        public void SetDominant(int v)   { Dominant = Mathf.Clamp(v, 0, 1);       PlayerPrefs.SetInt(KeyHandedness, Dominant); }

        private void LoadVR()
        {
            LocomotionMode = PlayerPrefs.GetInt(KeyLocomotion, 0);
            TurnMode       = PlayerPrefs.GetInt(KeySnapTurn,   1);
            Vignette       = PlayerPrefs.GetInt(KeyVignette,   1) == 1;
            Dominant       = PlayerPrefs.GetInt(KeyHandedness, 0);
        }
    }
}
