using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace HCITrilogy.Containment.Puzzles
{
    /// <summary>
    /// Color-coded grabbable cable. Carries its plug color so VRCableSocket
    /// can validate the connection.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class VRCable : MonoBehaviour
    {
        [SerializeField] private Color plugColor = Color.cyan;
        public Color PlugColor => plugColor;
        public bool Plugged;
    }
}
