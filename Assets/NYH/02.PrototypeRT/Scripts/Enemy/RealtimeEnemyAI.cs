using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public class RealtimeEnemyAI : MonoBehaviour
    {
        [Header("추적 대상")]
        [Tooltip("적이 따라갈 대상입니다. 비워두면 시작 시 Team이 Player인 Health를 자동으로 찾습니다.")]
        [SerializeField] private Transform target;

        [Header("이동")]
        [Tooltip("적이 대상을 향해 이동하는 속도입니다.")]
        [SerializeField, Min(0f)] private float moveSpeed = 2.2f;

        [Tooltip("이 거리 안에 플레이어가 들어오면 추적을 시작합니다.")]
        [SerializeField, Min(0f)] private float detectionRange = 6f;

        [Tooltip("이 거리보다 가까워지면 이동을 멈추고 공격 범위 안에 있다고 판단합니다.")]
        [SerializeField, Min(0f)] private float stopDistance = 0.75f;

        private Rigidbody2D _rigidbody;
        private Health _health;

        public bool HasTargetInAttackRange => target != null && Vector2.Distance(transform.position, target.position) <= stopDistance;
        public Transform Target => target;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _health = GetComponent<Health>();
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
        }

        private void Start()
        {
            if (target == null)
            {
                Health[] healths = FindObjectsByType<Health>(FindObjectsSortMode.None);
                foreach (Health health in healths)
                {
                    if (health.Team == Team.Player)
                    {
                        target = health.transform;
                        break;
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (PrototypeDungeonManager.IsRunEnded) return;
            if (_health.IsDead || target == null) return;

            Vector2 toTarget = target.position - transform.position;
            float distance = toTarget.magnitude;
            if (distance > detectionRange || distance <= stopDistance) return;

            // 추적 로직을 AI에만 두면 나중에 몬스터별 패턴을 자식 컴포넌트나 상태 enum으로 확장하기 쉽다.
            Vector2 next = _rigidbody.position + toTarget.normalized * (moveSpeed * Time.fixedDeltaTime);
            _rigidbody.MovePosition(next);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, stopDistance);
        }
    }
}
