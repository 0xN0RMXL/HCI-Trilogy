using UnityEngine;
using UnityEngine.UI;
using HCITrilogy.Lockdown.Core;

namespace HCITrilogy.Lockdown.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button menuButton;

        private void Awake()
        {
            if (panel) panel.SetActive(false);
            if (resumeButton) resumeButton.onClick.AddListener(() => PauseController.Instance?.Resume());
            if (menuButton)   menuButton.onClick.AddListener(() => { Time.timeScale = 1f; SceneFlow.Instance?.LoadAsync("MainMenu"); });
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
        private void OnPause(bool paused) { if (panel) panel.SetActive(paused); }
    }
}
