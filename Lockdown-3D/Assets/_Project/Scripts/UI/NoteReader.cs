using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using HCITrilogy.Lockdown.Core;
using HCITrilogy.Lockdown.Player;

namespace HCITrilogy.Lockdown.UI
{
    /// <summary>
    /// Full-screen overlay for paper notes. Static Show()/Hide() so any item
    /// can call from anywhere.
    /// Note: we deliberately do NOT call PauseController.Pause() here because
    /// the PauseMenu also subscribes to OnPauseChanged and would render its
    /// own overlay on top of the note. Instead we freeze look/move by toggling
    /// FirstPersonController.ControlEnabled directly.
    /// </summary>
    public class NoteReader : MonoBehaviour
    {
        public static NoteReader Instance { get; private set; }

        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;

        public static bool IsOpen { get; private set; }

        private static FirstPersonController _cachedFpc;

        private void Awake()
        {
            Instance = this;
            if (panel) panel.SetActive(false);
        }

        private static FirstPersonController GetFpc()
        {
            if (_cachedFpc != null) return _cachedFpc;
            _cachedFpc = Object.FindFirstObjectByType<FirstPersonController>();
            return _cachedFpc;
        }

        public static void Show(string title, string body)
        {
            if (Instance == null) return;
            if (Instance.titleText != null) Instance.titleText.text = title;
            if (Instance.bodyText != null) Instance.bodyText.text = body;
            if (Instance.panel != null) Instance.panel.SetActive(true);
            IsOpen = true;
            // Freeze player look/move without flipping the global pause flag,
            // which would also pop the PauseMenu over the note overlay.
            var fpc = GetFpc();
            if (fpc != null) fpc.ControlEnabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public static void Hide()
        {
            if (Instance == null) return;
            if (Instance.panel != null) Instance.panel.SetActive(false);
            IsOpen = false;
            var fpc = GetFpc();
            if (fpc != null) fpc.ControlEnabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (panel == null || !panel.activeSelf) return;
            if (Keyboard.current != null &&
                (Keyboard.current.escapeKey.wasPressedThisFrame ||
                 Keyboard.current.eKey.wasPressedThisFrame ||
                 Keyboard.current.spaceKey.wasPressedThisFrame))
                Hide();
        }
    }
}
