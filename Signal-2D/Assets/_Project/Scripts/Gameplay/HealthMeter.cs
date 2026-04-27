using System;
using UnityEngine;

namespace HCITrilogy.Signal.Gameplay
{
    public class HealthMeter : MonoBehaviour
    {
        public float Health { get; private set; } = 100f;
        public event Action<float> OnHealthChanged;
        public event Action OnFailed;

        [SerializeField] private float missPenalty   = 5f;
        [SerializeField] private float goodHealing   = 1f;
        [SerializeField] private float perfectHealing = 2f;

        private bool _failed;

        private void OnEnable() => FeedbackBus.OnFeedback += OnFeedback;
        private void OnDisable() => FeedbackBus.OnFeedback -= OnFeedback;

        private void OnFeedback(Judgment j, int laneIndex)
        {
            if (_failed) return;
            Health += j switch
            {
                Judgment.Perfect => perfectHealing,
                Judgment.Good    => goodHealing,
                Judgment.Miss    => -missPenalty,
                _                => 0
            };
            Health = Mathf.Clamp(Health, 0f, 100f);
            OnHealthChanged?.Invoke(Health);
            if (Health <= 0f)
            {
                _failed = true;
                OnFailed?.Invoke();
            }
        }
    }
}
