using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace HCITrilogy.Containment.Puzzles
{
    /// <summary>
    /// Wraps an XRSocketInteractor to validate that the inserted cable
    /// has the expected plug color before "accepting" it.
    /// HCI: direct manipulation. Hand-to-socket motion replaces a click.
    /// </summary>
    [RequireComponent(typeof(XRSocketInteractor))]
    public class VRCableSocket : MonoBehaviour
    {
        [SerializeField] private Color expectedColor = Color.cyan;
        [SerializeField] private AudioSource sfxConnect;
        [SerializeField] private AudioSource sfxReject;
        [SerializeField] private Light indicator;

        public event Action OnConnected;
        public bool Connected { get; private set; }

        private XRSocketInteractor _socket;

        private void Awake()
        {
            _socket = GetComponent<XRSocketInteractor>();
            if (indicator != null) indicator.color = new Color(1f, 0.36f, 0.36f);
            _socket.selectEntered.AddListener(OnInserted);
            _socket.selectExited.AddListener(OnRemoved);
        }

        private void OnDestroy()
        {
            _socket.selectEntered.RemoveListener(OnInserted);
            _socket.selectExited.RemoveListener(OnRemoved);
        }

        private void OnInserted(SelectEnterEventArgs e)
        {
            if (e.interactableObject is not XRBaseInteractable inter) return;
            var cable = inter.GetComponent<VRCable>();
            if (cable == null) return;
            if (ColorsClose(cable.PlugColor, expectedColor))
            {
                cable.Plugged = true;
                if (indicator != null) indicator.color = new Color(0.30f, 0.88f, 0.63f);
                sfxConnect?.Play();
                if (!Connected)
                {
                    Connected = true;
                    OnConnected?.Invoke();
                }
            }
            else
            {
                sfxReject?.Play();
            }
        }

        private void OnRemoved(SelectExitEventArgs e)
        {
            if (Connected) return;
            if (indicator != null) indicator.color = new Color(1f, 0.36f, 0.36f);
        }

        private static bool ColorsClose(Color a, Color b) =>
            Mathf.Abs(a.r - b.r) < 0.15f && Mathf.Abs(a.g - b.g) < 0.15f && Mathf.Abs(a.b - b.b) < 0.15f;
    }
}
