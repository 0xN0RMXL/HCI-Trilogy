using System;
using UnityEngine;
using HCITrilogy.Lockdown.Core;
using HCITrilogy.Lockdown.Interaction;

namespace HCITrilogy.Lockdown.Puzzles
{
    /// <summary>
    /// Final door. Only available once PuzzleStateMachine reports all puzzles
    /// solved. On interact, plays unlock sound + slides open + ends the run.
    /// </summary>
    public class DoorLock : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform door;
        [SerializeField] private float openSpeed = 1.6f;
        [SerializeField] private float openSlide = 2.0f;
        [SerializeField] private AudioSource sfxUnlock;
        [SerializeField] private AudioSource sfxLocked;
        [SerializeField] private PuzzleStateMachine state;

        public event Action OnUnlocked;
        public bool Unlocked { get; private set; }

        public string Prompt => Unlocked ? "[E] Open" : (state != null && state.AllSolved ? "[E] Unlock" : "Locked");
        public bool IsAvailable => state == null || state.AllSolved || Unlocked;

        private Vector3 _closed; private Vector3 _open; private float _t;
        private Highlightable _h;

        private void Awake()
        {
            if (door == null) door = transform;
            _closed = door.localPosition;
            _open = _closed + Vector3.right * openSlide;
            _h = GetComponentInChildren<Highlightable>();
        }

        public void Interact(Interactor by)
        {
            if (state != null && !state.AllSolved && !Unlocked)
            {
                sfxLocked?.Play();
                return;
            }
            if (!Unlocked)
            {
                Unlocked = true;
                sfxUnlock?.Play();
                OnUnlocked?.Invoke();
            }
        }

        private void Update()
        {
            if (!Unlocked) return;
            _t = Mathf.MoveTowards(_t, 1f, Time.deltaTime * openSpeed);
            door.localPosition = Vector3.Lerp(_closed, _open, _t);
        }

        public void Hover(bool on) => _h?.SetHover(on);
    }
}
