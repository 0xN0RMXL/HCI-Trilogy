using UnityEngine;
using HCITrilogy.Lockdown.Core;
using HCITrilogy.Lockdown.Interaction;

namespace HCITrilogy.Lockdown.Puzzles
{
    /// <summary>
    /// Slides a child along its local Z axis when interacted with.
    /// HCI: clear signifier (handle), simple toggle affordance.
    /// </summary>
    public class Drawer : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform body;
        [SerializeField] private float openDistance = 0.45f;
        [SerializeField] private float speed = 3.0f;
        [SerializeField] private AudioSource sfxOpen;
        [SerializeField] private AudioSource sfxClose;
        [SerializeField] private GameObject hiddenItem; // optional: spawned/visible when open

        private Vector3 _closed;
        private Vector3 _open;
        private bool _isOpen;
        private float _t;
        private Highlightable _highlight;

        public string Prompt => _isOpen ? LocalizationStrings.PromptInteract + " (close)" : LocalizationStrings.PromptInteract + " (open)";
        public bool IsAvailable => true;

        private void Awake()
        {
            if (body == null) body = transform;
            _closed = body.localPosition;
            _open = _closed + Vector3.forward * openDistance;
            _highlight = GetComponentInChildren<Highlightable>();
            if (hiddenItem != null) hiddenItem.SetActive(false);
        }

        private void Update()
        {
            _t = Mathf.MoveTowards(_t, _isOpen ? 1f : 0f, Time.deltaTime * speed);
            body.localPosition = Vector3.Lerp(_closed, _open, _t);
        }

        public void Interact(Interactor by)
        {
            _isOpen = !_isOpen;
            (_isOpen ? sfxOpen : sfxClose)?.Play();
            if (hiddenItem != null && _isOpen) hiddenItem.SetActive(true);
        }

        public void Hover(bool on) => _highlight?.SetHover(on);
    }
}
