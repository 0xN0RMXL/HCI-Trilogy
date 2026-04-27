using UnityEngine;
using HCITrilogy.Lockdown.Interaction;

namespace HCITrilogy.Lockdown.Puzzles
{
    public class KeypadButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private Keypad keypad;
        [SerializeField] private int digit;
        [SerializeField] private bool isClear;

        public string Prompt => isClear ? "[E] Clear" : $"[E] {digit}";
        public bool IsAvailable => keypad != null && !keypad.Solved;

        private Highlightable _h;
        private void Awake() => _h = GetComponentInChildren<Highlightable>();

        public void Interact(Interactor by)
        {
            if (keypad == null) return;
            if (isClear) keypad.Clear();
            else keypad.Press(digit);
        }

        public void Hover(bool on) => _h?.SetHover(on);
    }
}
