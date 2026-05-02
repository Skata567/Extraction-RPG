using UnityEngine;

namespace PrototypeRT
{
    public class PlayerAim2D : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform aimPivot;
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
