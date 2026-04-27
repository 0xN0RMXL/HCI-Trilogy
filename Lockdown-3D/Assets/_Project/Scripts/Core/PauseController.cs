using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HCITrilogy.Lockdown.Core
{
    /// <summary>
    /// Toggles pause via Esc. Sets Time.timeScale = 0, frees the cursor.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        public static PauseController Instance { get; private set; }
        public bool IsPaused { get; private set; }
        public event Action<bool> OnPauseChanged;

        [SerializeField] private InputActionAsset inputActions;
        private InputAction _pauseAction;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (inputActions != null)
            {
                _pauseAction = inputActions.FindAction("UI/Cancel", false)
                               ?? inputActions.FindAction("Player/Pause", false);
                if (_pauseAction != null)
                {
                    _pauseAction.Enable();
                    _pauseAction.started += _ => Toggle();
                }
            }
        }

        public void Toggle() { if (IsPaused) Resume(); else Pause(); }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            OnPauseChanged?.Invoke(true);
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            OnPauseChanged?.Invoke(false);
        }
    }
}
