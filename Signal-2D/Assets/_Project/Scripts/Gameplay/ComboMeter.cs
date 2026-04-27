using System;
using UnityEngine;

namespace HCITrilogy.Signal.Gameplay
{
    public class ComboMeter : MonoBehaviour
    {
        public int Combo { get; private set; }
        public int MaxCombo { get; private set; }
        public int Multiplier => 1 + Mathf.Min(Combo / 50, 3);

        public event Action<int, int> OnComboChanged; // current, multiplier

        private void OnEnable() => FeedbackBus.OnFeedback += OnFeedback;
        private void OnDisable() => FeedbackBus.OnFeedback -= OnFeedback;

        private void OnFeedback(Judgment j, int laneIndex)
        {
            if (j == Judgment.Miss)
            {
                if (Combo > 0) FeedbackBus.TriggerComboBreak();
                Combo = 0;
            }
            else
            {
                Combo++;
                if (Combo > MaxCombo) MaxCombo = Combo;
            }
            OnComboChanged?.Invoke(Combo, Multiplier);
        }
    }
}
