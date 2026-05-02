using System.Collections;
using UnityEngine;

namespace PrototypeRT
{
    public class DamageFlash2D : MonoBehaviour
    {
        [Header("피격 대상")]
        [Tooltip("피격 이벤트를 감지할 Health입니다. 비워두면 같은 오브젝트에서 자동으로 찾습니다.")]
        [SerializeField] private Health targetHealth;

        [Tooltip("피격 시 색을 바꿀 SpriteRenderer입니다. 비워두면 자식 포함해서 자동으로 찾습니다.")]
        [SerializeField] private SpriteRenderer targetRenderer;

        [Header("피격 표시")]
        [Tooltip("피격 순간에 바꿀 색상입니다.")]
        [SerializeField] private Color flashColor = Color.red;

        [Tooltip("피격 색상이 유지되는 시간입니다.")]
        [SerializeField, Min(0f)] private float flashDuration = 0.12f;

        private Color _originalColor = Color.white;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            if (targetHealth == null)
                targetHealth = GetComponent<Health>();
            if (targetRenderer == null)
                targetRenderer = GetComponentInChildren<SpriteRenderer>();
            if (targetRenderer != null)
                _originalColor = targetRenderer.color;
        }

        private void OnEnable()
        {
            PrototypeRTEvents.OnDamaged += HandleDamaged;
        }

        private void OnDisable()
        {
            PrototypeRTEvents.OnDamaged -= HandleDamaged;
            if (targetRenderer != null)
                targetRenderer.color = _originalColor;
        }

        private void HandleDamaged(Health damagedHealth, DamageInfo damageInfo)
        {
            if (damagedHealth != targetHealth || targetRenderer == null) return;

            // 같은 대상이 연속으로 맞을 때 이전 깜빡임을 끝까지 기다리지 않고 새 피격 피드백을 즉시 보여준다.
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);

            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            targetRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            targetRenderer.color = _originalColor;
            _flashRoutine = null;
        }
    }
}
