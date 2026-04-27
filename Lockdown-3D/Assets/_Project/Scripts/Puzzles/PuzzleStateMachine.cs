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

        public bool KeypadSolved { get; private set; }
        public bool DialSolved   { get; private set; }
        public bool SocketASolved { get; private set; }
        public bool SocketBSolved { get; private set; }

        public bool AllSolved =>
            KeypadSolved && DialSolved && SocketASolved && SocketBSolved;

        public event Action OnAllSolved;
        public event Action<string> OnPuzzleSolved;

        private void Start()
        {
            if (keypad != null)  keypad.OnAccepted += () => Set(ref KeypadSolved, "Keypad");
            if (dial != null)    dial.OnAligned   += () => Set(ref DialSolved,   "Dial");
            if (socketA != null) socketA.OnConnected += () => Set(ref SocketASolved, "Socket-A");
            if (socketB != null) socketB.OnConnected += () => Set(ref SocketBSolved, "Socket-B");
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
