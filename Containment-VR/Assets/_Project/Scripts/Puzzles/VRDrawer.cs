using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace HCITrilogy.Containment.Puzzles
{
    /// <summary>
    /// Hand-pullable drawer. While grabbed, the drawer follows the controller
    /// along its local Z axis, clamped between closed/open positions.
    ///
    /// HCI: direct manipulation. The handle is the affordance, the pull is
    /// the verb — no UI layer needed.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class VRDrawer : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private float openDistance = 0.45f;
        [SerializeField] private AudioSource sfxOpen;
        [SerializeField] private AudioSource sfxClose;

        private XRGrabInteractable _grab;
        private Transform _gripT;
        private Vector3 _closedLocal;
        private float _gripStartZLocal;
        private float _bodyStartZLocal;
        private bool _wasOpen;

        private void Awake()
        {
            if (body == null) body = transform;
            _closedLocal = body.localPosition;
            _grab = GetComponent<XRGrabInteractable>();
            _grab.movementType = XRBaseInteractable.MovementType.Kinematic;
            _grab.trackPosition = false;
            _grab.trackRotation = false;
            _grab.selectEntered.AddListener(OnGrab);
            _grab.selectExited.AddListener(OnRelease);
        }

        private void OnDestroy()
        {
            if (_grab == null) return;
            _grab.selectEntered.RemoveListener(OnGrab);
            _grab.selectExited.RemoveListener(OnRelease);
        }

        private void OnGrab(SelectEnterEventArgs e)
        {
            _gripT = e.interactorObject.transform;
            // local Z of the controller relative to drawer parent
            _gripStartZLocal = transform.parent != null
                ? transform.parent.InverseTransformPoint(_gripT.position).z
                : _gripT.position.z;
            _bodyStartZLocal = body.localPosition.z;
        }

        private void OnRelease(SelectExitEventArgs e)
        {
            _gripT = null;
            bool isOpen = body.localPosition.z - _closedLocal.z > openDistance * 0.5f;
            if (isOpen != _wasOpen)
            {
                (isOpen ? sfxOpen : sfxClose)?.Play();
                _wasOpen = isOpen;
            }
        }

        private void Update()
        {
            if (_gripT == null) return;
            float currentZ = transform.parent != null
                ? transform.parent.InverseTransformPoint(_gripT.position).z
                : _gripT.position.z;
            float delta = currentZ - _gripStartZLocal;
            float z = Mathf.Clamp(_bodyStartZLocal + delta, _closedLocal.z, _closedLocal.z + openDistance);
            body.localPosition = new Vector3(_closedLocal.x, _closedLocal.y, z);
        }
    }
}
