using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace HCITrilogy.Core.Editor
{
    /// <summary>
    /// Creates an AudioMixer asset and wires it into SettingsManager.
    /// Groups and exposed parameters must be added manually in the
    /// Audio Mixer window (Unity's API doesn't expose group/parameter
    /// creation). The setup menu prints instructions.
    /// </summary>
    public static class MixerFactory
    {
        /// <summary>
        /// Loads an existing GameMixer.mixer asset at the given folder and wires it
        /// into the SettingsManager on <paramref name="managersGO"/>. If the asset
        /// does not exist, logs a one-line instruction and returns null.
        ///
        /// Unity's public API exposes no way to create AudioMixer assets from code
        /// (AudioMixer is a native type, not a ScriptableObject, and there is no
        /// AudioMixer.Create() API). Users must create the asset once via
        /// Assets > Create > Audio Mixer, name it "GameMixer", add Master/SFX/Music
        /// groups, and expose volume parameters as MasterVolume/SFXVolume/MusicVolume.
        /// Subsequent setup-menu runs will auto-wire it.
        /// </summary>
        public static AudioMixer CreateOrLoad(string folder, GameObject managersGO)
        {
            const string assetName = "GameMixer.mixer";
            var fullPath = folder + "/" + assetName;
            var existing = AssetDatabase.LoadAssetAtPath<AudioMixer>(fullPath);
            if (existing != null)
            {
                WireIntoSettingsManager(managersGO, existing);
                return existing;
            }

            Debug.LogWarning("[HCI Trilogy] No GameMixer.mixer found at " + fullPath + ". " +
                             "Create it manually: Assets > Create > Audio Mixer, name it GameMixer, " +
                             "add Master/SFX/Music groups, then expose volume parameters named " +
                             "MasterVolume, SFXVolume, MusicVolume. Re-run the setup menu to auto-wire it.");
            return null;
        }

        private static void WireIntoSettingsManager(GameObject go, AudioMixer mixer)
        {
            if (go == null || mixer == null) return;
            var sm = go.GetComponent<HCITrilogy.Core.SettingsManager>();
            if (sm == null) return;
            var so = new SerializedObject(sm);
            so.FindProperty("mixer").objectReferenceValue = mixer;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
