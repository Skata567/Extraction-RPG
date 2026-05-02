using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(RealtimeEnemyAI))]
    public class EnemyAttack2D : MonoBehaviour
    {
        [SerializeField, Min(0)] private int damage = 1;
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
