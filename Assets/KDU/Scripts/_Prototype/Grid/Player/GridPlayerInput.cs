using UnityEngine;

/// <summary>
/// WASD/방향키 이동, Space 던지기, Q 포션. 플레이어 턴이 아닐 때 입력 무시.
/// </summary>
public class GridPlayerInput : MonoBehaviour
{
    private GridPlayer _player;

    public void Init(GridPlayer player)
    {
        _player = player;
    }

    private void Update()
    {
        if (TurnManager.Instance == null || !TurnManager.Instance.IsPlayerTurn) return;
        if (_player.Health.IsDead) return;
        if (!TimeSystem.Instance.IsRunning) return;

        // 이동 (단일 키 처리 — 동시 입력 시 위 우선)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            _player.Movement.TryMove(Vector2Int.up);
            return;
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            _player.Movement.TryMove(Vector2Int.down);
            return;
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _player.Movement.TryMove(Vector2Int.left);
            return;
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            _player.Movement.TryMove(Vector2Int.right);
            return;
        }

        // 단검 던지기
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _player.Combat.TryThrow();
            return;
        }

        // 포션 사용
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _player.Health.TryUsePotion();
            return;
        }
    }
}
