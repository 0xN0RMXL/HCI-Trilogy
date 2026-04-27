using System;
using UnityEngine;

namespace HCITrilogy.Containment.Puzzles
{
    public class VRPuzzleStateMachine : MonoBehaviour
    {
        [SerializeField] private VRKeypad keypad;
        [SerializeField] private VRDial   dial;
        [SerializeField] private VRCableSocket socketA;
        [SerializeField] private VRCableSocket socketB;

        // Backing fields rather than auto-properties: an auto-property cannot
        // be passed by ref (CS0206), and Set(ref _, name) requires a field.
        private bool _keypadSolved, _dialSolved, _socketASolved, _socketBSolved;
        public bool KeypadSolved  => _keypadSolved;
        public bool DialSolved    => _dialSolved;
        public bool SocketASolved => _socketASolved;
        public bool SocketBSolved => _socketBSolved;
        public bool AllSolved => _keypadSolved && _dialSolved && _socketASolved && _socketBSolved;

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
