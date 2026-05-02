using UnityEngine;

namespace PrototypeRT
{
    public interface IInteractable
    {
        bool CanInteract(GameObject interactor);
        void Interact(GameObject interactor);
    }
}
