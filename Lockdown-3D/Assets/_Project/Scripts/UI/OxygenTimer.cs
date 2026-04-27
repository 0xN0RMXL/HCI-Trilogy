using UnityEngine;
using UnityEngine.UI;

namespace HCITrilogy.Lockdown.UI
{
    /// <summary>
    /// Diegetic countdown timer rendered to a world-space canvas (a screen
    /// on the wall). When time runs out, raises OnExpired so the lab can
    /// trigger the failure end.
    /// HCI: diegetic UI — info lives in the world, not as a screen overlay.
    /// </summary>
    public class OxygenTimer : MonoBehaviour
    {
        [SerializeField] private float startSeconds = 480f; // 8 min
        [SerializeField] private Text label;
        public bool Running = true;
        public float TimeLeft { get; private set; }
        public bool Expired { get; private set; }
        public event System.Action OnExpired;

        private void Start() => TimeLeft = startSeconds;

        private void Update()
        {
            if (!Running || Expired) return;
            TimeLeft -= Time.deltaTime;
            if (TimeLeft <= 0f)
            {
                TimeLeft = 0f;
                Expired = true;
                OnExpired?.Invoke();
            }
            if (label != null)
            {
                int m = Mathf.FloorToInt(TimeLeft / 60f);
                int s = Mathf.FloorToInt(TimeLeft % 60f);
                label.text = $"{m:0}:{s:00}";
                label.color = TimeLeft < 60f ? new Color(1f, 0.36f, 0.36f) : new Color(0.36f, 0.88f, 1f);
            }
        }
    }
}
