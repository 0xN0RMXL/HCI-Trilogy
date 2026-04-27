using UnityEngine;
using HCITrilogy.Lockdown.Interaction;

namespace HCITrilogy.Lockdown.Puzzles
{
    /// <summary>
    /// A cable plug. Pick it up, then "use" it on a CableSocket of matching color
    /// (the socket consumes the plug from the Interactor's HeldPickup field).
    /// </summary>
    public class Cable : Pickupable
    {
        [SerializeField] private Color plugColor = Color.cyan;
        public Color PlugColor => plugColor;
        public bool   Plugged  { get; set; }
    }
}
