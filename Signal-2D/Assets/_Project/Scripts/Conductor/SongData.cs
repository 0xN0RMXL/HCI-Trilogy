using UnityEngine;

namespace HCITrilogy.Signal.Conductor
{
    /// <summary>
    /// ScriptableObject bundling an audio clip with its parsed chart JSON.
    /// Create via Assets → Create → Signal → Song Data.
    /// </summary>
    [CreateAssetMenu(menuName = "Signal/Song Data", fileName = "NewSong")]
    public class SongData : ScriptableObject
    {
        public AudioClip track;
        public string    title   = "Untitled";
        public string    artist  = "Unknown";
        [Tooltip("Beats per minute of the track.")]
        public float     bpm     = 120f;
        [Tooltip("Seconds of silence before the first beat.")]
        public float     offsetSeconds = 0f;
        [Tooltip("Scroll speed of notes in world-space units per second.")]
        public float     scrollSpeedUnitsPerSec = 6f;
        [Tooltip("JSON chart file. See Assets/_Project/Charts for the schema.")]
        public TextAsset chartJson;
    }
}
