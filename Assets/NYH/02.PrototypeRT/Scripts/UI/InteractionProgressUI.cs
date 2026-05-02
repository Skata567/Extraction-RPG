using UnityEngine;
using UnityEngine.UI;

namespace PrototypeRT
{
    public class InteractionProgressUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Text progressText;

        public void Configure(GameObject root, Slider slider, Text label)
        {
            panelRoot = root;
            progressSlider = slider;
            progressText = label;
            Hide();
        }

        private void Awake()
        {
            if (panelRoot == null)
                panelRoot = gameObject;
        }

        private void OnEnable()
        {
            PrototypeRTEvents.OnInteractionProgressStarted += HandleStarted;
            PrototypeRTEvents.OnInteractionProgressUpdated += HandleUpdated;
            PrototypeRTEvents.OnInteractionProgressCanceled += Hide;
            PrototypeRTEvents.OnInteractionProgressCompleted += Hide;
        }

        private void OnDisable()
        {
            PrototypeRTEvents.OnInteractionProgressStarted -= HandleStarted;
            PrototypeRTEvents.OnInteractionProgressUpdated -= HandleUpdated;
            PrototypeRTEvents.OnInteractionProgressCanceled -= Hide;
            PrototypeRTEvents.OnInteractionProgressCompleted -= Hide;
        }

        private void HandleStarted(string label, float duration)
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);
            if (progressText != null)
                progressText.text = label;
            if (progressSlider != null)
            {
                progressSlider.minValue = 0f;
                progressSlider.maxValue = 1f;
                progressSlider.value = 0f;
            }
        }

        private void HandleUpdated(float progress)
        {
            if (progressSlider != null)
                progressSlider.value = Mathf.Clamp01(progress);
        }

        private void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
            if (progressSlider != null)
                progressSlider.value = 0f;
        }
    }
}
