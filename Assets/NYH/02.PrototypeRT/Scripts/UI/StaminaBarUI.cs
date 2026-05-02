using UnityEngine;
using UnityEngine.UI;

namespace PrototypeRT
{
    public class StaminaBarUI : MonoBehaviour
    {
        [SerializeField] private Slider staminaSlider;
        [SerializeField] private Image fillImage;

        public void Configure(Slider slider, Image fill)
        {
            staminaSlider = slider;
            fillImage = fill;
        }

        private void Awake()
        {
            if (staminaSlider == null)
                staminaSlider = GetComponent<Slider>();
        }

        private void OnEnable()
        {
            PrototypeRTEvents.OnStaminaChanged += Refresh;
        }

        private void OnDisable()
        {
            PrototypeRTEvents.OnStaminaChanged -= Refresh;
        }

        private void Refresh(float current, float max)
        {
            float safeMax = Mathf.Max(1f, max);
            if (staminaSlider != null)
            {
                staminaSlider.minValue = 0f;
                staminaSlider.maxValue = safeMax;
                staminaSlider.value = Mathf.Clamp(current, 0f, safeMax);
            }

            // Slider와 Filled Image 중 어느 방식으로 UI를 만들든 재사용할 수 있게 둘 다 지원한다.
            if (fillImage != null)
                fillImage.fillAmount = Mathf.Clamp01(current / safeMax);
        }
    }
}
