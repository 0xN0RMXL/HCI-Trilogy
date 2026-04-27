using System;
using UnityEngine;
using HCITrilogy.Lockdown.Interaction;

namespace HCITrilogy.Lockdown.Puzzles
{
    /// <summary>
    /// Socket that accepts a Cable of the matching color. On match, snaps
    /// the cable in place and disables interaction.
    /// </summary>
    public class CableSocket : MonoBehaviour, IInteractable
    {
        [SerializeField] private Color expectedColor = Color.cyan;
        [SerializeField] private Transform snapPoint;
        [SerializeField] private AudioSource sfxConnect;
        [SerializeField] private Light indicator;

        public event Action OnConnected;
        public bool Connected { get; private set; }
        public string Prompt => Connected ? "" : "[E] Plug in cable";
        public bool IsAvailable => !Connected;

        private Highlightable _h;
        private void Awake()
        {
            _h = GetComponentInChildren<Highlightable>();
            if (indicator != null) indicator.color = new Color(1f, 0.36f, 0.36f);
        }

        public void Interact(Interactor by)
        {
            if (by == null || by.HeldPickup is not Cable cable) return;
            if (cable.Plugged) return;
            if (ColorsClose(cable.PlugColor, expectedColor))
            {
                cable.Drop(by);
                cable.Plugged = true;
                if (snapPoint != null)
                {
                    cable.transform.SetPositionAndRotation(snapPoint.position, snapPoint.rotation);
                    var rb = cable.GetComponent<Rigidbody>();
                    if (rb != null) rb.isKinematic = true;
                    foreach (var c in cable.GetComponentsInChildren<Collider>()) c.enabled = false;
                }
                Connected = true;
                if (indicator != null) indicator.color = new Color(0.30f, 0.88f, 0.63f);
                sfxConnect?.Play();
                OnConnected?.Invoke();
            }
        }

        public void Hover(bool on) => _h?.SetHover(on);

        private static bool ColorsClose(Color a, Color b) =>
            Mathf.Abs(a.r - b.r) < 0.15f && Mathf.Abs(a.g - b.g) < 0.15f && Mathf.Abs(a.b - b.b) < 0.15f;
    }
}
