using UnityEngine;

namespace PrototypeRT
{
    public interface IProgressInteractable
    {
        float InteractionDuration { get; }
        string InteractionPrompt { get; }
        string ProgressText { get; }

        bool CanContinueInteraction(GameObject interactor);
        void CompleteInteraction(GameObject interactor);
        void CancelInteraction(GameObject interactor);
    }
}
