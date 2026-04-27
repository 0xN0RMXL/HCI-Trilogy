using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace HCITrilogy.Containment.Puzzles
{
    /// <summary>
    /// VR rotatable dial. While grabbed, the dial's local Y angle tracks the
    /// difference between the grabbing controller's Y rotation and its
    /// rotation at grab-start.
    ///
    /// HCI: natural mapping (Norman). Twisting your wrist is the input;
    /// no virtual mapping layer between intent and action.
    /// </summary>
    [RequireComponent(typeof(XRBaseInteractable))]
    public class VRDial : MonoBehaviour
    {
        [SerializeField] private Transform pointer;
        [SerializeField] private int detents = 12;
        [SerializeField] private int targetIndex = 7;
        [SerializeField] private AudioSource sfxTick;
        [SerializeField] private AudioSource sfxAccept;

        public event Action OnAligned;
        public bool Solved { get; private set; }
        public int Index { get; private set; }

        private XRBaseInteractable _i;
        private Transform _gripT;
        private float _gripStartY;
        private float _angleAtGrab;
        private float _angleDeg;

        private void Awake()
        {
            _i = GetComponent<XRBaseInteractable>();
            _i.selectEntered.AddListener(OnGrab);
            _i.selectExited.AddListener(OnRelease);
        }

        private void OnGrab(SelectEnterEventArgs e)
        {
            if (Solved) return;
            _gripT = e.interactorObject.transform;
            _gripStartY = _gripT.eulerAngles.y;
            _angleAtGrab = _angleDeg;
        }

        private void OnRelease(SelectExitEventArgs e)
        {
            _gripT = null;
            if (Solved) return;
            if (Index == targetIndex)
            {
                Solved = true;
                sfxAccept?.Play();
                OnAligned?.Invoke();
            }
        }

        private void Update()
        {
            if (_gripT == null) return;
            float currentY = _gripT.eulerAngles.y;
            float delta = Mathf.DeltaAngle(_gripStartY, currentY);
            _angleDeg = _angleAtGrab + delta;
            if (pointer != null) pointer.localEulerAngles = new Vector3(0f, _angleDeg, 0f);
            int idx = Mathf.RoundToInt(_angleDeg / (360f / detents)) % detents;
            if (idx < 0) idx += detents;
            if (idx != Index) { Index = idx; sfxTick?.Play(); }
        }
    }
}
