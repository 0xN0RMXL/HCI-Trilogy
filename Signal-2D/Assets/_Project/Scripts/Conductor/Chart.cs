using System;

namespace HCITrilogy.Signal.Conductor
{
    /// <summary>Root JSON object for a song chart.</summary>
    [Serializable]
    public class Chart
    {
        public string title;
        public string artist;
        public float  bpm;
        public float  offsetSeconds;
        public NoteData[] notes;
    }
}
