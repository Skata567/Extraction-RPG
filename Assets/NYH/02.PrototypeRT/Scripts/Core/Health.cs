using System.Collections;
using UnityEngine;

namespace PrototypeRT
{
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("소속")]
        [Tooltip("이 오브젝트가 어느 편인지 정합니다. 플레이어는 Player, 적은 Enemy로 설정해야 아군 공격을 막을 수 있습니다.")]
        [SerializeField] private Team team = Team.Neutral;

        [Header("체력")]
        [Tooltip("최대 체력입니다. 시작 시 현재 체력도 이 값으로 초기화됩니다.")]
        [SerializeField] private int maxHp = 10;

        [Tooltip("체력이 0이 되었을 때 오브젝트를 삭제할지 정합니다. 플레이어는 재시작 흐름이 필요하면 켜두고, 특수 연출이 있으면 끌 수 있습니다.")]
        [SerializeField] private bool destroyOnDeath = true;

        public int CurrentHp { get; private set; }
        public int MaxHp => maxHp;
        public bool IsDead => CurrentHp <= 0;
        public bool IsInvulnerable { get; private set; }
        public Team Team => team;

        private Coroutine _invulnerabilityRoutine;

        private void Awake()
        {
            CurrentHp = Mathf.Max(1, maxHp);
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (IsDead || damageInfo.Amount <= 0) return;
            if (IsInvulnerable) return;
            if (damageInfo.SourceTeam == team && team != PrototypeRT.Team.Neutral) return;

            CurrentHp = Mathf.Max(0, CurrentHp - damageInfo.Amount);
            PrototypeRTEvents.RaiseDamaged(this, damageInfo);
            AudioManager.Instance?.PlaySfxByKey("Hit");

            if (IsDead)
            {
                PrototypeRTEvents.RaiseDied(this);

                // 사망 처리를 Health에 모아두면 적, 플레이어, 파괴 가능한 오브젝트가 같은 규칙을 공유할 수 있다.
                if (destroyOnDeath)
                    Destroy(gameObject);
            }
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0) return;
            int before = CurrentHp;
            CurrentHp = Mathf.Min(maxHp, CurrentHp + amount);
            int healedAmount = CurrentHp - before;
            if (healedAmount > 0)
                PrototypeRTEvents.RaiseHealed(this, healedAmount);
        }

        public void SetInvulnerable(float duration)
        {
            if (duration <= 0f || IsDead) return;

            // 대시, 회피, 향후 직업 스킬이 같은 무적 처리 경로를 쓰도록 Health 쪽에 모아둔다.
            if (_invulnerabilityRoutine != null)
                StopCoroutine(_invulnerabilityRoutine);

            _invulnerabilityRoutine = StartCoroutine(InvulnerabilityRoutine(duration));
        }

        private IEnumerator InvulnerabilityRoutine(float duration)
        {
            IsInvulnerable = true;
            yield return new WaitForSeconds(duration);
            IsInvulnerable = false;
            _invulnerabilityRoutine = null;
        }
    }
}
