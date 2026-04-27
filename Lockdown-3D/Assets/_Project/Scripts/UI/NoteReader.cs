using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using HCITrilogy.Lockdown.Core;

namespace HCITrilogy.Lockdown.UI
{
    /// <summary>
    /// Full-screen overlay for paper notes. Static Show()/Hide() so any item
    /// can call from anywhere.
    /// </summary>
    public class NoteReader : MonoBehaviour
    {
        public static NoteReader Instance { get; private set; }

        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;

        private void Awake()
        {
            Instance = this;
            if (panel) panel.SetActive(false);
        }

        public static void Show(string title, string body)
        {
            if (Instance == null) return;
            Instance.titleText.text = title;
            Instance.bodyText.text = body;
            Instance.panel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public static void Hide()
        {
            if (Instance == null) return;
            Instance.panel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!panel.activeSelf) return;
            if (Keyboard.current != null &&
                (Keyboard.current.escapeKey.wasPressedThisFrame ||
                 Keyboard.current.eKey.wasPressedThisFrame ||
                 Keyboard.current.spaceKey.wasPressedThisFrame))
                Hide();
        }
    }
}
