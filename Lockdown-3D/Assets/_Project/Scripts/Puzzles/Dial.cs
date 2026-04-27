using System;
using UnityEngine;
using UnityEngine.InputSystem;
using HCITrilogy.Lockdown.Interaction;

namespace HCITrilogy.Lockdown.Puzzles
{
    /// <summary>
    /// Rotatable dial with N detents. While interacted, the player's mouse-X
    /// scrolls the dial; release [E] to confirm.
    /// HCI: maps a continuous rotation to N discrete states (constraints).
    /// </summary>
    public class Dial : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform pointer;
        [SerializeField] private int detents = 12;
        [SerializeField] private int targetIndex = 7;
        [SerializeField] private float degreesPerMouseUnit = 0.6f;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private AudioSource sfxTick;
        [SerializeField] private AudioSource sfxAccept;

        public event Action OnAligned;
        public bool Solved { get; private set; }
        public int Index { get; private set; }

        public string Prompt => Solved ? "" : "[E] Turn (mouse-X)";
        public bool IsAvailable => !Solved;

        private bool _engaged;
        private float _angleDeg;
        private InputAction _look;
        private InputAction _interact;
        private Highlightable _h;

        private void Awake() => _h = GetComponentInChildren<Highlightable>();

        private void OnEnable()
        {
            if (inputActions != null)
            {
                var map = inputActions.FindActionMap("Player", false);
                if (map != null)
                {
                    _look = map.FindAction("Look", false);
                    _interact = map.FindAction("Interact", false);
                }
            }
        }

        public void Interact(Interactor by)
        {
            _engaged = !_engaged;
            if (_engaged) _engagedTime = Time.unscaledTime;
            else Confirm();
        }

        private void Update()
        {
            if (Solved) return;
            if (_engaged && _look != null)
            {
                float dx = _look.ReadValue<Vector2>().x;
                _angleDeg += dx * degreesPerMouseUnit;
                int idx = Mathf.RoundToInt(_angleDeg / (360f / detents)) % detents;
                if (idx < 0) idx += detents;
                if (idx != Index) { Index = idx; sfxTick?.Play(); }
                if (pointer != null) pointer.localEulerAngles = new Vector3(0f, _angleDeg, 0f);
            }

            if (_engaged && _interact != null && _interact.WasPressedThisFrame() && !ReferenceFrameWasJustEngaged())
                Confirm();
        }

        // Avoid immediate re-confirm in same frame as engage.
        private float _engagedTime;
        private bool ReferenceFrameWasJustEngaged() => Time.unscaledTime - _engagedTime < 0.15f;

        private void Confirm()
        {
            _engaged = false;
            if (Index == targetIndex)
            {
                Solved = true;
                sfxAccept?.Play();
                OnAligned?.Invoke();
            }
        }

        public void Hover(bool on) => _h?.SetHover(on);
    }
}
