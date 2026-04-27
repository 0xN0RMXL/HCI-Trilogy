using UnityEngine;
using UnityEngine.UI;
using HCITrilogy.Lockdown.Core;

namespace HCITrilogy.Lockdown.UI
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
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            int min = Mathf.FloorToInt(LastEscapeSeconds / 60f);
            int sec = Mathf.FloorToInt(LastEscapeSeconds % 60f);
            if (timeText)   timeText.text   = $"{min:00}:{sec:00}";
            if (resultText) resultText.text = LastSuccess ? "ESCAPED" : "FAILED";
            if (retryButton) retryButton.onClick.AddListener(() => SceneFlow.Instance?.LoadAsync("Lab"));
            if (menuButton)  menuButton.onClick.AddListener(()  => SceneFlow.Instance?.LoadAsync("MainMenu"));
        }
    }
}
