using UnityEngine;

/// <summary>
/// 추적자: 시야 안에 플레이어 들어오면 1칸씩 이동, 인접 시 다음 턴 공격 의도 표시.
/// 턴 흐름:
///   1. 이전 턴 표시한 공격 의도 발동 (플레이어가 그 칸에 있으면 데미지)
///   2. 시야 안이면 플레이어 방향으로 1칸 이동
///   3. 인접하면 다음 턴 공격 의도(플레이어 현재 칸) 표시
/// </summary>
public class GridEnemyTracker : GridEnemyBase
{
    private Vector2Int? _pendingAttackCell;

    public override void OnTurnUpdate()
    {
        if (IsDead || Player == null) return;

        // 1. 이전 턴 공격 의도 발동
        if (_pendingAttackCell.HasValue && Player.GridPosition == _pendingAttackCell.Value)
        {
            Player.TakeDamage(data.attackDamage);
        }
        _pendingAttackCell = null;
        ClearTelegraphs();

        // 2. 시야 안이면 플레이어 방향으로 1칸 이동
        if (SquaredDistanceToPlayer() <= data.visionRange * data.visionRange)
        {
            TryStepTowardPlayer();
        }

        // 3. 인접 시 다음 턴 공격 의도 표시
        if (IsAdjacentToPlayer())
        {
            _pendingAttackCell = Player.GridPosition;
            ShowTelegraph(_pendingAttackCell.Value);
        }
    }

    private void TryStepTowardPlayer()
    {
        var diff = Player.GridPosition - GridPosition;

        var dirX = diff.x != 0 ? new Vector2Int(System.Math.Sign(diff.x), 0) : Vector2Int.zero;
        var dirY = diff.y != 0 ? new Vector2Int(0, System.Math.Sign(diff.y)) : Vector2Int.zero;

        // 더 먼 축 우선, 차단 시 다른 축 시도
        bool xPrimary = Mathf.Abs(diff.x) >= Mathf.Abs(diff.y);
        var primary = xPrimary ? dirX : dirY;
        var secondary = xPrimary ? dirY : dirX;

        if (primary != Vector2Int.zero && TryStep(primary)) return;
        if (secondary != Vector2Int.zero && TryStep(secondary)) return;
    }

    private bool TryStep(Vector2Int dir)
    {
        var next = GridPosition + dir;
        if (!GridSystem.Instance.IsWalkable(next)) return false;
        if (next == Player.GridPosition) return false;  // 플레이어 칸으로는 이동 안 함 (공격은 텔레그래프로)
        if (TurnManager.Instance.GetEntityAt(next) != null) return false;  // 다른 엔티티 점유

        SetGridPosition(next);
        UpdateVisibility();
        return true;
    }
}
