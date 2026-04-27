using System;
using UnityEngine;

namespace HCITrilogy.Containment.Puzzles
{
    /// <summary>
    /// Final exit door. When all puzzles are solved, the door auto-slides open
    /// over a couple of seconds. (Originally planned as a hand-pull lever, but
    /// for course-friendly demoability, automatic opening keeps the demo
    /// short and reliable across hardware.)
    /// </summary>
    public class VRDoor : MonoBehaviour
    {
        [SerializeField] private Transform door;
        [SerializeField] private float openSpeed = 1.4f;
        [SerializeField] private float openSlide = 2.2f;
        [SerializeField] private AudioSource sfxUnlock;
        [SerializeField] private VRPuzzleStateMachine state;

        public event Action OnOpened;
        private Vector3 _closed; private Vector3 _open;
        private float _t;
        private bool _opening;
        private bool _opened;

        private void Awake()
        {
            if (door == null) door = transform;
            _closed = door.localPosition;
            _open = _closed + Vector3.right * openSlide;
        }

        private void Start()
        {
            if (state != null) state.OnAllSolved += BeginOpen;
        }

        private void OnDestroy()
        {
            if (state != null) state.OnAllSolved -= BeginOpen;
        }

        private void BeginOpen()
        {
            if (_opening) return;
            _opening = true;
            sfxUnlock?.Play();
        }

        private void Update()
        {
            if (!_opening || _opened) return;
            _t = Mathf.MoveTowards(_t, 1f, Time.deltaTime * openSpeed);
            door.localPosition = Vector3.Lerp(_closed, _open, _t);
            if (_t >= 1f)
            {
                _opened = true;
                OnOpened?.Invoke();
            }
        }
    }
}
