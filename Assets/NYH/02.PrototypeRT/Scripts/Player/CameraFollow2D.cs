using UnityEngine;

namespace PrototypeRT
{
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("추적 대상")]
        [Tooltip("카메라가 따라갈 대상입니다. 비워두면 PlayerMovement2D가 붙은 플레이어를 자동으로 찾습니다.")]
        [SerializeField] private Transform target;

        [Header("카메라 위치")]
        [Tooltip("대상 위치에서 얼마나 떨어져 카메라를 둘지 정합니다. 2D 카메라는 보통 Z를 -10으로 둡니다.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        [Tooltip("카메라가 대상을 따라가는 부드러움입니다. 0에 가까울수록 즉시 따라갑니다.")]
        [SerializeField, Min(0f)] private float followSmoothTime = 0.12f;

        private Vector3 _velocity;

        private void Start()
        {
            if (target == null)
            {
                PlayerMovement2D player = FindFirstObjectByType<PlayerMovement2D>();
                if (player != null)
                    target = player.transform;
            }

            if (target == null)
                Debug.LogError("CameraFollow2D: 따라갈 Player가 없습니다. Main Camera의 Target에 Player를 연결하세요.");
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // 플레이어 이동이 FixedUpdate에서 끝난 뒤 카메라를 갱신해야 화면 떨림이 덜하다.
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, followSmoothTime);
        }
    }
}
