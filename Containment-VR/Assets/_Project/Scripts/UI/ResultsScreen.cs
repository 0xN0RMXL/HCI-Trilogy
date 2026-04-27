using UnityEngine;
using UnityEngine.UI;
using HCITrilogy.Containment.Core;

namespace HCITrilogy.Containment.UI
{
    public class ResultsScreen : MonoBehaviour
    {
        [SerializeField] private Text timeText;
        [SerializeField] private Text resultText;
        public static float LastEscapeSeconds;
        public static bool LastSuccess;

        private void Start()
        {
            int min = Mathf.FloorToInt(LastEscapeSeconds / 60f);
            int sec = Mathf.FloorToInt(LastEscapeSeconds % 60f);
            if (timeText)   timeText.text   = $"{min:00}:{sec:00}";
            if (resultText) resultText.text = LastSuccess ? "ESCAPED" : "FAILED";
        }

        public void OnRetryButton() => SceneFlow.Instance?.LoadAsync("Lab");
        public void OnMenuButton()  => SceneFlow.Instance?.LoadAsync("MainMenu");
    }
}
