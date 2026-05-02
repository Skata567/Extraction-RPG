using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(Health))]
    public class PlayerPotionUser : MonoBehaviour
    {
        [SerializeField] private KeyCode useKey = KeyCode.Q;
        [SerializeField, Min(0)] private int startPotionCount = 1;
        [SerializeField, Min(1)] private int maxPotionCount = 3;
        [SerializeField, Range(0f, 1f)] private float healMaxHpRatio = 0.4f;
        [SerializeField, Min(0f)] private float useCooldown = 1f;

        private Health _health;
        private int _currentPotionCount;
        private float _nextUseTime;

        public int CurrentPotionCount => _currentPotionCount;
        public int MaxPotionCount => maxPotionCount;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _currentPotionCount = Mathf.Clamp(startPotionCount, 0, maxPotionCount);
        }

        private void Start()
        {
            RaiseChanged();
        }

        private void Update()
        {
            if (PrototypeDungeonManager.IsRunEnded) return;
            if (Input.GetKeyDown(useKey))
                UsePotion();
        }

        public bool UsePotion()
        {
            if (_health == null || _health.IsDead) return false;
            if (Time.time < _nextUseTime) return false;
            if (_currentPotionCount <= 0) return false;

            int healAmount = Mathf.CeilToInt(_health.MaxHp * healMaxHpRatio);
            if (healAmount <= 0) return false;

            // v0.1에서는 포션을 그리드 아이템으로 만들지 않고, 전투 생존감 검증용 고정 자원으로 둔다.
            _currentPotionCount--;
            _nextUseTime = Time.time + useCooldown;
            _health.Heal(healAmount);
            RaiseChanged();
            AudioManager.Instance?.PlaySfxByKey("Potion");
            return true;
        }

        public void ResetPotions()
        {
            _currentPotionCount = Mathf.Clamp(startPotionCount, 0, maxPotionCount);
            _nextUseTime = 0f;
            RaiseChanged();
        }

        private void RaiseChanged()
        {
            PrototypeRTEvents.RaisePotionCountChanged(_currentPotionCount, maxPotionCount);
        }
    }
}
