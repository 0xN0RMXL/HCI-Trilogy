using UnityEngine;
using UnityEngine.UI;
using HCITrilogy.Signal.Core;
using HCITrilogy.Signal.Gameplay;

namespace HCITrilogy.Signal.UI
{
    /// <summary>
    /// Live score / combo / health / song-progress readout. Subscribes to gameplay events.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text comboText;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image songProgressFill;
        [SerializeField] private ScoreManager score;
        [SerializeField] private ComboMeter combo;
        [SerializeField] private HealthMeter health;

        private Conductor.Conductor _conductor;

        private void OnEnable()
        {
            if (score != null)  score.OnScoreChanged   += UpdateScore;
            if (combo != null)  combo.OnComboChanged   += UpdateCombo;
            if (health != null) health.OnHealthChanged += UpdateHealth;
        }

        private void OnDisable()
        {
            if (score != null)  score.OnScoreChanged   -= UpdateScore;
            if (combo != null)  combo.OnComboChanged   -= UpdateCombo;
            if (health != null) health.OnHealthChanged -= UpdateHealth;
        }

        private void Start()
        {
            _conductor = ServiceLocator.Get<Conductor.Conductor>();
            if (songProgressFill != null) songProgressFill.fillAmount = 0f;
        }

        private void Update()
        {
            if (songProgressFill == null || _conductor == null || _conductor.Song == null) return;
            var clip = _conductor.Song.track;
            if (clip == null || clip.length <= 0f) return;
            float p = Mathf.Clamp01((float)(_conductor.SongPositionSeconds / clip.length));
            songProgressFill.fillAmount = p;
        }

        private void UpdateScore(int s)
        {
            if (scoreText != null) scoreText.text = s.ToString("D6");
        }

        private void UpdateCombo(int c, int mul)
        {
            if (comboText == null) return;
            if (c < 2) comboText.text = "";
            else comboText.text = $"x{c}  ×{mul}";
        }

        private void UpdateHealth(float h)
        {
            if (healthFill != null) healthFill.fillAmount = h / 100f;
        }
    }
}
