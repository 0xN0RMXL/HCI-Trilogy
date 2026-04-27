using UnityEngine;
using UnityEngine.UI;
using HCITrilogy.Containment.Core;

namespace HCITrilogy.Containment.UI
{
    public class ResultsScreen : MonoBehaviour
    {
        [SerializeField] private Text timeText;
        [SerializeField] private Text resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;
        public static float LastEscapeSeconds;
        public static bool LastSuccess;

        private void Start()
        {
            int min = Mathf.FloorToInt(LastEscapeSeconds / 60f);
            int sec = Mathf.FloorToInt(LastEscapeSeconds % 60f);
            if (timeText)   timeText.text   = $"{min:00}:{sec:00}";
            if (resultText) resultText.text = LastSuccess ? "ESCAPED" : "FAILED";
            if (retryButton) retryButton.onClick.AddListener(OnRetryButton);
            if (menuButton)  menuButton.onClick.AddListener(OnMenuButton);
        }

        public void OnRetryButton()
        {
            if (SceneFlow.Instance != null) SceneFlow.Instance.LoadAsync("Lab");
            else UnityEngine.SceneManagement.SceneManager.LoadScene("Lab");
        }

        public void OnMenuButton()
        {
            if (SceneFlow.Instance != null) SceneFlow.Instance.LoadAsync("MainMenu");
            else UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
