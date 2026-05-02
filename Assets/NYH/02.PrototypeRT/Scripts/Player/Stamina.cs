using UnityEngine;

namespace PrototypeRT
{
    public class Stamina : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxStamina = 100f;
        [SerializeField, Min(0f)] private float recoverPerSecond = 25f;
        [SerializeField, Min(0f)] private float recoverDelay = 0.35f;

        private float _currentStamina;
        private float _recoverBlockedUntil;

        public float CurrentStamina => _currentStamina;
        public float MaxStamina => maxStamina;

        private void Awake()
        {
            _currentStamina = maxStamina;
        }

        private void Start()
        {
            RaiseChanged();
        }

        private void Update()
        {
            if (PrototypeDungeonManager.IsRunEnded) return;
            Recover();
        }

        public bool TrySpend(float amount)
        {
            if (amount <= 0f) return true;
            if (_currentStamina < amount) return false;

            // 대시, 공격, 나중의 직업 스킬이 모두 같은 소비 API를 쓰면 밸런스 조정 지점이 한 곳으로 모인다.
            _currentStamina = Mathf.Max(0f, _currentStamina - amount);
            _recoverBlockedUntil = Time.time + recoverDelay;
            RaiseChanged();
            return true;
        }

        public void ResetStamina()
        {
            _currentStamina = maxStamina;
            _recoverBlockedUntil = 0f;
            RaiseChanged();
        }

        private void Recover()
        {
            if (Time.time < _recoverBlockedUntil) return;
            if (_currentStamina >= maxStamina) return;

            _currentStamina = Mathf.Min(maxStamina, _currentStamina + recoverPerSecond * Time.deltaTime);
            RaiseChanged();
        }

        private void RaiseChanged()
        {
            PrototypeRTEvents.RaiseStaminaChanged(_currentStamina, maxStamina);
        }
    }
}
