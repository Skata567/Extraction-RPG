using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(PlayerAim2D))]
    public class PlayerCombat2D : MonoBehaviour
    {
        [SerializeField, Min(0)] private int baseDamage = 1;
        [SerializeField, Min(0f)] private float attackRadius = 0.8f;
        [SerializeField, Range(1f, 360f)] private float attackAngle = 110f;
        [SerializeField, Min(0f)] private float attackCooldown = 0.45f;
        [SerializeField, Min(0f)] private float attackStaminaCost;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private PlayerEquipment playerEquipment;
        [SerializeField] private Stamina stamina;
        [SerializeField] private Transform attackOrigin;

        private PlayerAim2D _aim;
        private float _nextAttackTime;

        private void Awake()
        {
            _aim = GetComponent<PlayerAim2D>();
            if (playerEquipment == null)
                playerEquipment = GetComponent<PlayerEquipment>();
            if (stamina == null)
                stamina = GetComponent<Stamina>();
            if (attackOrigin == null)
                attackOrigin = transform;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                TryAttack();
        }

        private void TryAttack()
        {
            if (PrototypeDungeonManager.IsRunEnded) return;
            if (Time.time < _nextAttackTime) return;
            if (attackStaminaCost > 0f && stamina != null && !stamina.TrySpend(attackStaminaCost)) return;

            _nextAttackTime = Time.time + attackCooldown;

            int finalDamage = CalculateDamage();
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackOrigin.position, attackRadius, targetLayers);

            foreach (Collider2D hit in hits)
            {
                if (hit.attachedRigidbody != null && hit.attachedRigidbody.gameObject == gameObject) continue;
                if (!IsInsideAttackArc(hit.transform.position)) continue;

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null || damageable.Team == Team.Player) continue;

                damageable.TakeDamage(new DamageInfo(finalDamage, gameObject, Team.Player));
            }
        }

        private int CalculateDamage()
        {
            // 나중에 장비, 직업, 유전자 보정치를 더할 수 있도록 데미지 계산을 별도 메서드로 분리한다.
            int equipmentBonus = playerEquipment != null ? playerEquipment.AttackBonus : 0;
            return Mathf.Max(0, baseDamage + equipmentBonus);
        }

        private bool IsInsideAttackArc(Vector3 targetPosition)
        {
            Vector2 toTarget = targetPosition - attackOrigin.position;
            if (toTarget.sqrMagnitude < 0.001f) return true;

            float angle = Vector2.Angle(_aim.AimDirection, toTarget.normalized);
            return angle <= attackAngle * 0.5f;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin, attackRadius);

            // Scene 뷰에서 실제 공격 방향을 볼 수 있게 해두면 전투 감각 튜닝 속도가 빨라진다.
            Vector2 aimDirection = Application.isPlaying && _aim != null ? _aim.AimDirection : Vector2.right;
            Gizmos.DrawLine(origin, origin + (Vector3)aimDirection * attackRadius);
        }
    }
}
