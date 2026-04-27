using System;
using UnityEngine;

namespace HCITrilogy.Lockdown.Puzzles
{
    /// <summary>
    /// Tracks puzzle completion. Other systems subscribe to OnSolved /
    /// OnAllSolved. Wiring (which puzzle reports which event) is done in the
    /// editor SetupMenu.
    /// </summary>
    public class PuzzleStateMachine : MonoBehaviour
    {
        [SerializeField] private Keypad keypad;
        [SerializeField] private Dial   dial;
        [SerializeField] private CableSocket socketA;
        [SerializeField] private CableSocket socketB;

        // Backing fields, not auto-properties: an auto-property cannot be
        // passed by ref (CS0206), and the original Set(ref _, name) helper
        // requires a real field.
        private bool _keypadSolved, _dialSolved, _socketASolved, _socketBSolved;

        public bool KeypadSolved  => _keypadSolved;
        public bool DialSolved    => _dialSolved;
        public bool SocketASolved => _socketASolved;
        public bool SocketBSolved => _socketBSolved;

        public bool AllSolved =>
            _keypadSolved && _dialSolved && _socketASolved && _socketBSolved;

        public event Action OnAllSolved;
        public event Action<string> OnPuzzleSolved;

        private void Start()
        {
            if (keypad != null)  keypad.OnAccepted   += () => Set(ref _keypadSolved,  "Keypad");
            if (dial != null)    dial.OnAligned      += () => Set(ref _dialSolved,    "Dial");
            if (socketA != null) socketA.OnConnected += () => Set(ref _socketASolved, "Socket-A");
            if (socketB != null) socketB.OnConnected += () => Set(ref _socketBSolved, "Socket-B");
        }

        private void Set(ref bool flag, string name)
        {
            if (flag) return;
            flag = true;
            OnPuzzleSolved?.Invoke(name);
            if (AllSolved) OnAllSolved?.Invoke();
        }
    }
}
