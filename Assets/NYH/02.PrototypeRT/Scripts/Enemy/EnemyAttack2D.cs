using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(RealtimeEnemyAI))]
    public class EnemyAttack2D : MonoBehaviour
    {
        [Header("공격 수치")]
        [Tooltip("적이 플레이어에게 한 번 공격할 때 주는 피해량입니다.")]
        [SerializeField, Min(0)] private int damage = 1;

        [Tooltip("공격 후 다음 공격까지 기다리는 시간입니다.")]
        [SerializeField, Min(0f)] private float attackCooldown = 1.1f;

        private RealtimeEnemyAI _ai;
        private float _nextAttackTime;

        private void Awake()
        {
            _ai = GetComponent<RealtimeEnemyAI>();
        }

        private void Update()
        {
            if (PrototypeDungeonManager.IsRunEnded) return;
            if (Time.time < _nextAttackTime || !_ai.HasTargetInAttackRange) return;

            Health targetHealth = _ai.Target.GetComponentInParent<Health>();
            if (targetHealth == null || targetHealth.Team != Team.Player) return;

            _nextAttackTime = Time.time + attackCooldown;
            targetHealth.TakeDamage(new DamageInfo(damage, gameObject, Team.Enemy));
        }
    }
}
