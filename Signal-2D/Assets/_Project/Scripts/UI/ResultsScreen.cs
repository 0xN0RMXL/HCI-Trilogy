using UnityEngine;
using UnityEngine.UI;
using HCITrilogy.Signal.Core;
using HCITrilogy.Signal.Gameplay;

namespace HCITrilogy.Signal.UI
{
    public class ResultsScreen : MonoBehaviour
    {
        [SerializeField] private Text perfectText;
        [SerializeField] private Text goodText;
        [SerializeField] private Text missText;
        [SerializeField] private Text maxComboText;
        [SerializeField] private Text accuracyText;
        [SerializeField] private Text gradeText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;

        public static int LastScore;
        public static int LastPerfect, LastGood, LastMiss, LastMaxCombo;

        private void Start()
        {
            int total = LastPerfect + LastGood + LastMiss;
            float acc = total > 0 ? (LastPerfect + LastGood * 0.5f) / total : 0f;

            if (perfectText)  perfectText.text  = LastPerfect.ToString();
            if (goodText)     goodText.text     = LastGood.ToString();
            if (missText)     missText.text     = LastMiss.ToString();
            if (maxComboText) maxComboText.text = LastMaxCombo.ToString();
            if (scoreText)    scoreText.text    = LastScore.ToString("D6");
            if (accuracyText) accuracyText.text = $"{acc * 100f:F1}%";
            if (gradeText)    gradeText.text    = GradeOf(acc);

            if (retryButton) retryButton.onClick.AddListener(() => SceneFlow.Instance?.LoadAsync("Game"));
            if (menuButton)  menuButton.onClick.AddListener(()  => SceneFlow.Instance?.LoadAsync("MainMenu"));
        }

        private static string GradeOf(float accuracy01)
        {
            if (accuracy01 >= 0.98f) return "S";
            if (accuracy01 >= 0.90f) return "A";
            if (accuracy01 >= 0.75f) return "B";
            if (accuracy01 >= 0.50f) return "C";
            return "F";
        }
    }
}
