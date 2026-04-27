using UnityEngine;
using HCITrilogy.Lockdown.Interaction;
using HCITrilogy.Lockdown.UI;

namespace HCITrilogy.Lockdown.Puzzles
{
    /// <summary>
    /// World-space paper note. On Interact, opens the NoteReader full-screen
    /// overlay and pauses player look. ESC / E closes it.
    /// </summary>
    public class NoteItem : MonoBehaviour, IInteractable
    {
        [SerializeField, TextArea(3, 12)] private string contents = "";
        [SerializeField] private string title = "Memo";

        public string Prompt => "[E] Read";
        public bool IsAvailable => true;

        private Highlightable _h;
        private void Awake() => _h = GetComponentInChildren<Highlightable>();

        public void Interact(Interactor by) => NoteReader.Show(title, contents);
        public void Hover(bool on) => _h?.SetHover(on);
    }
}
