using UnityEngine;

namespace HCITrilogy.Signal.Gameplay
{
    /// <summary>
    /// Simple screen-shake on hit. Pseudo-haptic feedback channel — stands in
    /// for the real haptics VR has but desktop does not.
    /// </summary>
    public class CameraShaker : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float perfectMag = 0.05f;
        [SerializeField] private float goodMag    = 0.03f;
        [SerializeField] private float missMag    = 0.12f;
        [SerializeField] private float decay = 14f;

        private Vector3 _basePos;
        private float _trauma;

        private void Awake()
        {
            if (cameraTransform == null) cameraTransform = transform;
            _basePos = cameraTransform.localPosition;
        }

        private void OnEnable() => FeedbackBus.OnFeedback += OnFeedback;
        private void OnDisable() => FeedbackBus.OnFeedback -= OnFeedback;

        private void OnFeedback(Judgment j, int laneIndex)
        {
            _trauma = Mathf.Max(_trauma, j switch
            {
                Judgment.Perfect => perfectMag,
                Judgment.Good    => goodMag,
                Judgment.Miss    => missMag,
                _ => 0f
            });
        }

        private void Update()
        {
            if (_trauma <= 0f)
            {
                cameraTransform.localPosition = _basePos;
                return;
            }
            float t = _trauma * _trauma; // quadratic feel
            Vector3 offset = new(
                (Random.value - 0.5f) * 2f * t,
                (Random.value - 0.5f) * 2f * t,
                0f);
            cameraTransform.localPosition = _basePos + offset;
            _trauma = Mathf.Max(0, _trauma - decay * Time.deltaTime);
        }
    }
}
