using System.Collections;
using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerMovement2D))]
    public class PlayerDash2D : MonoBehaviour
    {
        [SerializeField] private KeyCode dashKey = KeyCode.Space;
        [SerializeField, Min(0f)] private float staminaCost = 30f;
        [SerializeField, Min(0.01f)] private float dashDuration = 0.16f;
        [SerializeField, Min(0f)] private float dashSpeed = 12f;
        [SerializeField, Min(0f)] private float invulnerableDuration = 0.18f;

        private Rigidbody2D _rigidbody;
        private PlayerMovement2D _movement;
        private Stamina _stamina;
        private Health _health;
        private bool _isDashing;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _movement = GetComponent<PlayerMovement2D>();
            _stamina = GetComponent<Stamina>();
            _health = GetComponent<Health>();
        }

        private void Update()
        {
            if (PrototypeDungeonManager.IsRunEnded || _isDashing) return;
            if (Input.GetKeyDown(dashKey))
                TryDash();
        }

        private void TryDash()
        {
            if (_health != null && _health.IsDead) return;
            if (_stamina != null && !_stamina.TrySpend(staminaCost)) return;

            Vector2 direction = _movement.MoveInput.sqrMagnitude > 0.001f
                ? _movement.MoveInput.normalized
                : _movement.LastMoveDirection.normalized;

            if (direction.sqrMagnitude < 0.001f)
                direction = Vector2.down;

            StartCoroutine(DashRoutine(direction));
        }

        private IEnumerator DashRoutine(Vector2 direction)
        {
            _isDashing = true;
            _movement.SetMovementLocked(true);
            _health?.SetInvulnerable(invulnerableDuration);
            AudioManager.Instance?.PlaySfxByKey("Dash");

            float elapsed = 0f;
            while (elapsed < dashDuration)
            {
                float step = dashSpeed * Time.fixedDeltaTime;
                _rigidbody.MovePosition(_rigidbody.position + direction * step);
                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            _movement.SetMovementLocked(false);
            _isDashing = false;
        }
    }
}
