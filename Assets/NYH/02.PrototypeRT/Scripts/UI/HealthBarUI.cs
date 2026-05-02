using UnityEngine;
using UnityEngine.UI;

namespace PrototypeRT
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Health targetHealth;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Image hpFill;

        private void Start()
        {
            if (targetHealth == null)
            {
                Debug.LogError("HealthBarUI: Target Health가 연결되지 않았습니다.");
                return;
            }

            Refresh();
        }

        private void OnEnable()
        {
            PrototypeRTEvents.OnDamaged += HandleDamaged;
            PrototypeRTEvents.OnHealed += HandleHealed;
            PrototypeRTEvents.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            PrototypeRTEvents.OnDamaged -= HandleDamaged;
            PrototypeRTEvents.OnHealed -= HandleHealed;
            PrototypeRTEvents.OnDied -= HandleDied;
        }

        private void HandleDamaged(Health damagedHealth, DamageInfo damageInfo)
        {
            if (damagedHealth == targetHealth)
                Refresh();
        }

        private void HandleDied(Health deadHealth)
        {
            if (deadHealth == targetHealth)
                Refresh();
        }

        private void HandleHealed(Health healedHealth, int amount)
        {
            if (healedHealth == targetHealth)
                Refresh();
        }

        private void Refresh()
        {
            if (targetHealth == null) return;

            float ratio = targetHealth.MaxHp <= 0 ? 0f : (float)targetHealth.CurrentHp / targetHealth.MaxHp;

            if (hpSlider != null)
            {
                hpSlider.minValue = 0f;
                hpSlider.maxValue = targetHealth.MaxHp;
                hpSlider.value = targetHealth.CurrentHp;
            }

            // Slider를 쓰지 않고 Image Type=Filled 방식으로도 연결할 수 있게 열어둔다.
            if (hpFill != null)
                hpFill.fillAmount = Mathf.Clamp01(ratio);
        }
    }
}
