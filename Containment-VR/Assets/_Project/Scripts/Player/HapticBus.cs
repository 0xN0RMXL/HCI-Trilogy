using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace HCITrilogy.Containment.Player
{
    /// <summary>
    /// Fires controller haptics on select/hover/activate. Drop on any
    /// XRBaseInteractable to make it tactile.
    ///
    /// HCI: in VR, haptics is a first-class feedback channel — discrete
    /// buzzes confirm "you grabbed something" without breaking presence
    /// the way a UI toast would.
    /// </summary>
    public class HapticBus : MonoBehaviour
    {
        [SerializeField] private float hoverAmplitude = 0.15f;
        [SerializeField] private float hoverDuration  = 0.05f;
        [SerializeField] private float selectAmplitude = 0.55f;
        [SerializeField] private float selectDuration  = 0.10f;

        private XRGrabInteractable _i;

        private void Awake()
        {
            _i = GetComponent<XRGrabInteractable>();
            if (_i == null) return;
            _i.firstHoverEntered.AddListener(OnHover);
            _i.selectEntered.AddListener(OnSelect);
        }

        private void OnDestroy()
        {
            if (_i == null) return;
            _i.firstHoverEntered.RemoveListener(OnHover);
            _i.selectEntered.RemoveListener(OnSelect);
        }

        private void OnHover(HoverEnterEventArgs args)  => Pulse(args.interactorObject, hoverAmplitude, hoverDuration);
        private void OnSelect(SelectEnterEventArgs args) => Pulse(args.interactorObject, selectAmplitude, selectDuration);

        private static void Pulse(IXRInteractor interactor, float amp, float dur)
        {
            // XRIT 3.x: XRBaseControllerInteractor exposes SendHapticImpulse directly.
            if (interactor is XRBaseControllerInteractor controller)
                controller.SendHapticImpulse(amp, dur);
        }
    }
}
