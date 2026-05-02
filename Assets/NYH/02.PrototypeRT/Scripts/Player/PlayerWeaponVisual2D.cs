using UnityEngine;

namespace PrototypeRT
{
    public class PlayerWeaponVisual2D : MonoBehaviour
    {
        [Header("조준 입력")]
        [Tooltip("마우스 방향을 계산하는 PlayerAim2D입니다. 비워두면 부모 또는 같은 오브젝트에서 자동으로 찾습니다.")]
        [SerializeField] private PlayerAim2D playerAim;

        [Header("무기 오브젝트")]
        [Tooltip("회전시킬 무기 Transform입니다. 보통 WeaponVisual 오브젝트 자신을 넣습니다. 비워두면 이 오브젝트의 Transform을 사용합니다.")]
        [SerializeField] private Transform weaponRoot;

        [Tooltip("좌우 반전을 적용할 무기 SpriteRenderer입니다. PlayerEquipment의 Weapon Renderer와 같은 SpriteRenderer를 넣어주세요.")]
        [SerializeField] private SpriteRenderer weaponRenderer;

        [Tooltip("무기 위치를 따라갈 기준점입니다. 비워두면 부모 Transform을 사용합니다.")]
        [SerializeField] private Transform followOrigin;

        [Header("회전 보정")]
        [Tooltip("스프라이트 원본 방향이 오른쪽을 향하지 않을 때 보정할 각도입니다. 오른쪽 기준 스프라이트라면 0으로 둡니다.")]
        [SerializeField] private float rotationOffset;

        [Tooltip("마우스가 왼쪽 반원에 있을 때 무기 스프라이트를 좌우 반전할지 정합니다. 일반적인 오른쪽 방향 무기 스프라이트는 켜두는 것이 자연스럽습니다.")]
        [SerializeField] private bool flipXOnLeftSide = true;

        [Tooltip("무기 오브젝트를 기준점 위치에 계속 붙여둘지 정합니다. WeaponVisual이 Player 자식이면 켜두는 것을 권장합니다.")]
        [SerializeField] private bool followOriginPosition = true;

        private void Awake()
        {
            if (weaponRoot == null)
                weaponRoot = transform;
            if (weaponRenderer == null)
                weaponRenderer = GetComponentInChildren<SpriteRenderer>();
            if (playerAim == null)
                playerAim = GetComponentInParent<PlayerAim2D>();
            if (followOrigin == null && transform.parent != null)
                followOrigin = transform.parent;
        }

        private void LateUpdate()
        {
            if (playerAim == null || weaponRoot == null) return;

            Vector2 aimDirection = playerAim.AimDirection;
            if (aimDirection.sqrMagnitude < 0.001f) return;

            if (followOriginPosition && followOrigin != null)
                weaponRoot.position = followOrigin.position;

            float rawAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            bool isLeftSide = aimDirection.x < 0f;

            // 왼쪽 반원에서는 스프라이트를 뒤집고 각도를 180도 보정해 무기가 한 바퀴 뒤집혀 보이지 않게 한다.
            float visualAngle = isLeftSide ? rawAngle + 180f : rawAngle;
            visualAngle = NormalizeAngle(visualAngle + rotationOffset);

            weaponRoot.rotation = Quaternion.Euler(0f, 0f, visualAngle);

            if (weaponRenderer != null)
                weaponRenderer.flipX = flipXOnLeftSide && isLeftSide;
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle <= -180f) angle += 360f;
            return angle;
        }
    }
}
