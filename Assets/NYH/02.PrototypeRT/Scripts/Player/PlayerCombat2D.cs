using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(PlayerAim2D))]
    public class PlayerCombat2D : MonoBehaviour
    {
        [Header("공격 수치")]
        [Tooltip("무기 보너스를 제외한 플레이어 기본 공격력입니다. 최종 피해량은 기본 공격력 + 장착 무기 보너스로 계산됩니다.")]
        [SerializeField, Min(0)] private int baseDamage = 1;

        [Tooltip("공격이 닿는 원형 범위입니다. 값이 작으면 적에게 거의 붙어서 클릭해야 맞습니다.")]
        [SerializeField, Min(0f)] private float attackRadius = 0.8f;

        [Tooltip("마우스 조준 방향을 기준으로 공격이 인정되는 부채꼴 각도입니다. 360이면 주변 전체를 공격합니다.")]
        [SerializeField, Range(1f, 360f)] private float attackAngle = 110f;

        [Tooltip("공격 후 다음 공격까지 기다리는 시간입니다. 낮을수록 빠르게 연속 공격할 수 있습니다.")]
        [SerializeField, Min(0f)] private float attackCooldown = 0.45f;

        [Tooltip("공격 한 번에 소비할 스태미나입니다. 0이면 스태미나를 소모하지 않습니다.")]
        [SerializeField, Min(0f)] private float attackStaminaCost;

        [Header("공격 대상")]
        [Tooltip("공격으로 맞출 수 있는 레이어입니다. 적 오브젝트가 Enemy 레이어라면 Enemy를 선택해야 합니다.")]
        [SerializeField] private LayerMask targetLayers;

        [Header("참조 연결")]
        [Tooltip("플레이어의 장비 컴포넌트입니다. 비워두면 같은 오브젝트에서 자동으로 찾습니다.")]
        [SerializeField] private PlayerEquipment playerEquipment;

        [Tooltip("플레이어의 스태미나 컴포넌트입니다. 공격 스태미나 비용을 사용할 때 필요하며, 비워두면 같은 오브젝트에서 자동으로 찾습니다.")]
        [SerializeField] private Stamina stamina;

        [Tooltip("공격 범위의 중심으로 사용할 위치입니다. 비워두면 플레이어 Transform을 기준으로 공격합니다.")]
        [SerializeField] private Transform attackOrigin;

        private PlayerAim2D _aim;
        private PlayerInteractor _interactor;
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
            _interactor = GetComponent<PlayerInteractor>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                TryAttack();
        }

        private void TryAttack()
        {
            if (PrototypeDungeonManager.IsRunEnded) return;
            if (_interactor != null && _interactor.IsInteractionInProgress) return;
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
