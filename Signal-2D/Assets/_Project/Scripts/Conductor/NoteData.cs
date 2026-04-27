using System;

namespace HCITrilogy.Signal.Conductor
{
    /// <summary>
    /// Plain serializable note: which lane, which beat, optional hold length.
    /// </summary>
    [Serializable]
    public struct NoteData
    {
        public int lane;        // 0..3
        public float beat;      // beat number from start of song
        public float holdBeats; // 0 = tap; >0 = hold (held for this many beats)
    }
}
