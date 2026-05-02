using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement2D : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 4f;

        private Rigidbody2D _rigidbody;
        private Vector2 _moveInput;
        private Vector2 _lastMoveDirection = Vector2.down;

        public Vector2 MoveInput => _moveInput;
        public Vector2 LastMoveDirection => _lastMoveDirection;
        public bool IsMovementLocked { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
        }

        private void Update()
        {
            ReadMoveInput();
        }

        private void FixedUpdate()
        {
            // Rigidbody2D 이동으로 처리해야 TilemapCollider2D 같은 물리 벽과 안정적으로 충돌한다.
            if (IsMovementLocked || PrototypeDungeonManager.IsRunEnded) return;

            _rigidbody.MovePosition(_rigidbody.position + _moveInput * (moveSpeed * Time.fixedDeltaTime));
        }

        public void SetMovementLocked(bool value)
        {
            // 대시 중에는 일반 이동과 대시 이동이 동시에 Rigidbody2D를 움직이지 않게 잠근다.
            IsMovementLocked = value;
            if (value)
                _moveInput = Vector2.zero;
        }

        private void ReadMoveInput()
        {
            if (IsMovementLocked || PrototypeDungeonManager.IsRunEnded)
            {
                _moveInput = Vector2.zero;
                return;
            }

            _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (_moveInput.sqrMagnitude > 1f)
                _moveInput.Normalize();
            if (_moveInput.sqrMagnitude > 0.001f)
                _lastMoveDirection = _moveInput.normalized;
        }
    }
}
