using UnityEngine;
using UnityEngine.UI;
using HCITrilogy.Lockdown.Core;

namespace HCITrilogy.Lockdown.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            if (playButton)     playButton.onClick.AddListener(() => SceneFlow.Instance?.LoadAsync("Lab"));
            if (settingsButton) settingsButton.onClick.AddListener(() => Toggle(settingsPanel));
            if (creditsButton)  creditsButton.onClick.AddListener(() => Toggle(creditsPanel));
            if (quitButton)     quitButton.onClick.AddListener(OnQuit);
            if (settingsPanel) settingsPanel.SetActive(false);
            if (creditsPanel)  creditsPanel.SetActive(false);
        }

        private static void Toggle(GameObject g) { if (g != null) g.SetActive(!g.activeSelf); }

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
