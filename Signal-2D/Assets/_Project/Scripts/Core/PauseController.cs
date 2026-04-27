using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HCITrilogy.Signal.Core
{
    /// <summary>
    /// Pauses/resumes the game. For Signal, we do NOT set Time.timeScale = 0
    /// because the Conductor uses AudioSettings.dspTime independently.
    /// Instead we raise events; the Conductor pauses itself via its own logic.
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
                _pauseAction = inputActions.FindAction("Gameplay/Pause", false)
                               ?? inputActions.FindAction("UI/Cancel", false);
                if (_pauseAction != null)
                {
                    _pauseAction.Enable();
                    _pauseAction.started += OnPausePressed;
                }
            }
        }

        private void OnDestroy()
        {
            if (_pauseAction != null) _pauseAction.started -= OnPausePressed;
        }

        private void OnPausePressed(InputAction.CallbackContext _) => Toggle();

        public void Toggle() { if (IsPaused) Resume(); else Pause(); }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            OnPauseChanged?.Invoke(true);
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            OnPauseChanged?.Invoke(false);
        }
    }
}
