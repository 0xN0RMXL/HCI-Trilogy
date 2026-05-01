using UnityEngine;
using HCITrilogy.Lockdown.Core;
using HCITrilogy.Lockdown.Interaction;

namespace HCITrilogy.Lockdown.Puzzles
{
    /// <summary>
    /// Pickup-and-carry-in-front-of-camera item. While carried, follows the
    /// camera's forward offset. Press Q (Drop) to release.
    /// </summary>
    public class Pickupable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "object";
        [SerializeField] private float carryDistance = 0.7f;
        [SerializeField] private float carryHeight = -0.05f;
        [SerializeField] private float lerpSpeed = 14f;

        public string Prompt => $"[E] Pick up {displayName}";
        public bool IsAvailable => !_carried;

        private Highlightable _highlight;
        private Rigidbody _rb;
        private Collider[] _cols;
        private bool _carried;
        private Camera _carryCam;
        private static Camera s_cachedMainCam;

        private void Awake()
        {
            _highlight = GetComponentInChildren<Highlightable>();
            _rb = GetComponent<Rigidbody>();
            _cols = GetComponentsInChildren<Collider>();
        }

        public void Interact(Interactor by)
        {
            if (_carried) return;
            _carried = true;
            _carryCam = ResolveMainCamera();
            if (_rb != null) { _rb.isKinematic = true; }
            foreach (var c in _cols) c.enabled = false;
            by.HeldPickup = this;
        }

        private static Camera ResolveMainCamera()
        {
            if (s_cachedMainCam != null) return s_cachedMainCam;
            s_cachedMainCam = Camera.main;
            return s_cachedMainCam;
        }

        public void Drop(Interactor by)
        {
            if (!_carried) return;
            _carried = false;
            if (_rb != null) { _rb.isKinematic = false; }
            foreach (var c in _cols) c.enabled = true;
            if (by != null) by.HeldPickup = null;
        }

        private void LateUpdate()
        {
            if (!_carried || _carryCam == null) return;
            Vector3 target = _carryCam.transform.position
                             + _carryCam.transform.forward * carryDistance
                             + _carryCam.transform.up * carryHeight;
            transform.position = Vector3.Lerp(transform.position, target, lerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, _carryCam.transform.rotation, lerpSpeed * Time.deltaTime);
        }

        public void Hover(bool on) => _highlight?.SetHover(on);
    }
}
