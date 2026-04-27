using UnityEngine;

namespace HCITrilogy.Lockdown.Interaction
{
    /// <summary>
    /// Anything the player can look at and press [E] on.
    /// HCI: a single interface gives the system one consistent affordance.
    /// </summary>
    public interface IInteractable
    {
        string Prompt { get; }
        bool   IsAvailable { get; }
        void   Interact(Interactor by);
        void   Hover(bool on);
    }
}
