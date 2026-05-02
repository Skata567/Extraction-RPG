using UnityEngine;

namespace PrototypeRT
{
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
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
