using UnityEngine;

namespace PrototypeRT
{
    public class PlayerAim2D : MonoBehaviour
    {
        [Header("조준 기준")]
        [Tooltip("마우스 위치를 월드 좌표로 변환할 카메라입니다. 비워두면 Main Camera를 자동으로 사용합니다.")]
        [SerializeField] private Camera targetCamera;

        [Tooltip("조준 방향의 기준점입니다. 무기 피벗을 따로 만들었다면 연결하고, 없으면 플레이어 위치를 기준으로 조준합니다.")]
        [SerializeField] private Transform aimPivot;

        [Tooltip("조준 방향에 맞춰 Aim Pivot을 회전시킬지 정합니다. 무기 표시 오브젝트를 회전시키고 싶을 때 켭니다.")]
        [SerializeField] private bool rotateAimPivot = true;

        public Vector2 AimDirection { get; private set; } = Vector2.right;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        private void Update()
        {
            UpdateAimDirection();
        }

        private void UpdateAimDirection()
        {
            if (targetCamera == null)
            {
                Debug.LogError("PlayerAim2D: Camera 참조가 없습니다. Main Camera 태그 또는 Inspector 연결을 확인하세요.");
                return;
            }

            Vector3 mouseWorld = targetCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 origin = aimPivot != null ? aimPivot.position : transform.position;
            Vector2 direction = mouseWorld - origin;
            if (direction.sqrMagnitude < 0.001f) return;

            AimDirection = direction.normalized;

            if (!rotateAimPivot || aimPivot == null) return;

            // 플레이어 본체 대신 무기 피벗만 돌린다. 캐릭터 방향 애니메이션은 나중에 별도 스프라이트/애니메이터에서 처리한다.
            float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
            aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
