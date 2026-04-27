using UnityEngine;
using UnityEngine.UI;
using HCITrilogy.Signal.Core;

namespace HCITrilogy.Signal.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button calibrateButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;

        private void Start()
        {
            if (playButton != null)      playButton.onClick.AddListener(OnPlay);
            if (calibrateButton != null) calibrateButton.onClick.AddListener(OnCalibrate);
            if (settingsButton != null)  settingsButton.onClick.AddListener(() => Toggle(settingsPanel));
            if (creditsButton != null)   creditsButton.onClick.AddListener(() => Toggle(creditsPanel));
            if (quitButton != null)      quitButton.onClick.AddListener(OnQuit);
            if (settingsPanel != null)   settingsPanel.SetActive(false);
            if (creditsPanel != null)    creditsPanel.SetActive(false);
        }

        private static void Toggle(GameObject panel) { if (panel != null) panel.SetActive(!panel.activeSelf); }

        private static void OnPlay()
        {
            if (SceneFlow.Instance != null) SceneFlow.Instance.LoadAsync("Game");
            else UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }

        private static void OnCalibrate()
        {
            if (SceneFlow.Instance != null) SceneFlow.Instance.LoadAsync("Calibration");
            else UnityEngine.SceneManagement.SceneManager.LoadScene("Calibration");
        }

        private static void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
