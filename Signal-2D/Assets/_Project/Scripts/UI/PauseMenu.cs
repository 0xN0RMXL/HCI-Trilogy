using UnityEngine;
using UnityEngine.UI;
using HCITrilogy.Signal.Core;

namespace HCITrilogy.Signal.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button menuButton;

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (resumeButton)   resumeButton.onClick.AddListener(() => PauseController.Instance?.Resume());
            if (settingsButton && settingsPanel != null)
                settingsButton.onClick.AddListener(() => settingsPanel.SetActive(!settingsPanel.activeSelf));
            if (menuButton)     menuButton.onClick.AddListener(() => SceneFlow.Instance?.LoadAsync("MainMenu"));
        }

        private void OnEnable()
        {
            if (PauseController.Instance != null)
                PauseController.Instance.OnPauseChanged += OnPause;
        }

        private void OnDisable()
        {
            if (PauseController.Instance != null)
                PauseController.Instance.OnPauseChanged -= OnPause;
        }

        private void OnPause(bool paused)
        {
            if (panel != null) panel.SetActive(paused);
        }
    }
}
