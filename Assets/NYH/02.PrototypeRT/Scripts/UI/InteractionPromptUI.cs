using UnityEngine;
using UnityEngine.UI;

namespace PrototypeRT
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private PlayerInteractor playerInteractor;
        [SerializeField] private Text promptText;
        [SerializeField] private string prompt = "E 상호작용";

        public void Configure(PlayerInteractor interactor, Text text)
        {
            playerInteractor = interactor;
            promptText = text;
        }

        private void Awake()
        {
            if (promptText == null)
                promptText = GetComponent<Text>();
            if (playerInteractor == null)
                playerInteractor = FindFirstObjectByType<PlayerInteractor>();
        }

        private void Update()
        {
            if (promptText == null) return;

            bool shouldShow = playerInteractor != null
                && playerInteractor.HasInteractable
                && !playerInteractor.IsInteractionInProgress
                && !PrototypeDungeonManager.IsRunEnded;

            // 상호작용 대상의 종류별 문구는 나중에 확장하고, v0.1에서는 키 안내만 안정적으로 보여준다.
            promptText.gameObject.SetActive(shouldShow);
            if (shouldShow)
            {
                if (playerInteractor.CurrentInteractable is IProgressInteractable progressInteractable)
                    promptText.text = progressInteractable.InteractionPrompt;
                else
                    promptText.text = prompt;
            }
        }
    }
}
