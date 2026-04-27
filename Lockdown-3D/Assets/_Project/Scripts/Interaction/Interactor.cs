using UnityEngine;
using UnityEngine.InputSystem;
using HCITrilogy.Lockdown.Core;
using HCITrilogy.Lockdown.UI;

namespace HCITrilogy.Lockdown.Interaction
{
    /// <summary>
    /// Camera-forward raycast for interactables. Emits prompt updates and
    /// invokes Interact() on press.
    ///
    /// HCI: visible crosshair signifier + dynamic prompt text encodes
    /// affordance ("you CAN press E here") rather than relying on
    /// the player to remember a generic key.
    /// </summary>
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private LayerMask interactionMask = ~0;
        [SerializeField] private float maxDistance = 2.6f;
        [SerializeField] private CrosshairPrompt crosshair;

        private InputAction _interact;
        private InputAction _drop;
        private IInteractable _hovered;

        // Held pickup is the only "global" interaction state we expose to
        // puzzles like CableSocket. Kept simple — a single slot.
        public HCITrilogy.Lockdown.Puzzles.Pickupable HeldPickup;

        private void OnEnable()
        {
            if (inputActions != null)
            {
                var map = inputActions.FindActionMap("Player", false);
                if (map != null)
                {
                    _interact = map.FindAction("Interact", false);
                    _drop     = map.FindAction("Drop", false);
                    map.Enable();
                }
            }
        }

        private void Update()
        {
            if (PauseController.Instance != null && PauseController.Instance.IsPaused) return;
            // While a paper note is open the player has no view control and
            // shouldn't be able to interact through the overlay.
            if (NoteReader.IsOpen) { crosshair?.SetPrompt(null); return; }
            UpdateRay();
            if (_interact != null && _interact.WasPressedThisFrame() && _hovered != null && _hovered.IsAvailable)
                _hovered.Interact(this);
            if (_drop != null && _drop.WasPressedThisFrame() && HeldPickup != null)
                HeldPickup.Drop(this);
        }

        private void UpdateRay()
        {
            if (viewCamera == null) viewCamera = Camera.main;
            if (viewCamera == null) return;

            var ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            IInteractable found = null;
            if (Physics.Raycast(ray, out var hit, maxDistance, interactionMask, QueryTriggerInteraction.Collide))
                found = hit.collider.GetComponentInParent<IInteractable>();
            if (found != _hovered)
            {
                _hovered?.Hover(false);
                _hovered = found;
                _hovered?.Hover(true);
            }
            crosshair?.SetPrompt(_hovered != null && _hovered.IsAvailable ? _hovered.Prompt : null);
        }
    }
}
