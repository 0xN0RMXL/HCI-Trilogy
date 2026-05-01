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
        /// Creates a GameMixer.mixer asset if one doesn't exist, and wires
        /// it into the SettingsManager on the given GameObject. Returns the mixer.
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

            if (!AssetDatabase.IsValidFolder(folder))
            {
                var parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
                var leaf = System.IO.Path.GetFileName(folder);
                AssetDatabase.CreateFolder(parent, leaf);
            }

            var mixer = ScriptableObject.CreateInstance<AudioMixer>();
            AssetDatabase.CreateAsset(mixer, fullPath);
            AssetDatabase.SaveAssets();

            WireIntoSettingsManager(managersGO, mixer);

            Debug.Log("[HCI Trilogy] Created GameMixer.mixer. Open the Audio Mixer window " +
                      "(Window > Audio > Audio Mixer) and add Master/SFX/Music groups with " +
                      "exposed volume parameters: MasterVolume, SFXVolume, MusicVolume.");
            return mixer;
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
