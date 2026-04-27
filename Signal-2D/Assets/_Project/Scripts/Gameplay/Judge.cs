using System;
using UnityEngine;
using HCITrilogy.Signal.Core;

namespace HCITrilogy.Signal.Gameplay
{
    /// <summary>
    /// Central scoring brain. Converts (lane press, first-unjudged note, song time)
    /// → Judgment and raises FeedbackBus.OnFeedback. Also handles timeout-miss.
    ///
    /// HCI principle: the windows (±40 ms Perfect, ±100 ms Good) match empirical
    /// just-noticeable-difference thresholds for audio-visual sync.
    /// </summary>
    public class Judge : MonoBehaviour
    {
        public static Judge Instance { get; private set; }

        [Header("Timing windows (seconds)")]
        [SerializeField] private double perfectWindow = 0.040;
        [SerializeField] private double goodWindow    = 0.100;

        public int PerfectCount { get; private set; }
        public int GoodCount    { get; private set; }
        public int MissCount    { get; private set; }

        public event Action<Judgment, int> OnJudged;
        public event Action OnAllNotesResolved;

        private Conductor.Conductor _c;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            _c = ServiceLocator.Get<Conductor.Conductor>();
        }

        public void JudgePress(Lane lane, bool silent = false)
        {
            if (_c == null) return;
            var note = lane.PeekFirstUnjudged();
            if (note == null) return;
            double err = (note.Data.beat - _c.SongPositionBeats) * _c.SecondsPerBeat;
            double absErr = Math.Abs(err);
            if (absErr <= perfectWindow) Resolve(note, Judgment.Perfect);
            else if (absErr <= goodWindow) Resolve(note, Judgment.Good);
            // Otherwise: too early, ignore (unless called silently from hold-to-play).
        }

        public void JudgeMiss(Note n) => Resolve(n, Judgment.Miss);

        private void Resolve(Note n, Judgment j)
        {
            switch (j)
            {
                case Judgment.Perfect: PerfectCount++; break;
                case Judgment.Good:    GoodCount++;    break;
                case Judgment.Miss:    MissCount++;    break;
            }
            int lane = n.Data.lane;
            n.MarkJudged();
            OnJudged?.Invoke(j, lane);
            FeedbackBus.Trigger(j, lane);
        }
    }
}
