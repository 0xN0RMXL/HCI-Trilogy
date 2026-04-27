using System;
using UnityEngine;

namespace HCITrilogy.Signal.Gameplay
{
    /// <summary>
    /// Sums up points. Subscribes to FeedbackBus. Combo multiplier grows from
    /// the ComboMeter so score depends on sustained performance, rewarding
    /// engagement (Nacke & Lindley 2008, on flow states).
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public int Score { get; private set; }
        public event Action<int> OnScoreChanged;

        [SerializeField] private ComboMeter combo;

        [SerializeField] private int perfectBase = 300;
        [SerializeField] private int goodBase    = 100;

        private void OnEnable() => FeedbackBus.OnFeedback += OnFeedback;
        private void OnDisable() => FeedbackBus.OnFeedback -= OnFeedback;

        private void OnFeedback(Judgment j, int laneIndex)
        {
            int baseScore = j switch
            {
                Judgment.Perfect => perfectBase,
                Judgment.Good    => goodBase,
                _                => 0
            };
            int mul = combo != null ? combo.Multiplier : 1;
            Score += baseScore * mul;
            OnScoreChanged?.Invoke(Score);
        }
    }
}
