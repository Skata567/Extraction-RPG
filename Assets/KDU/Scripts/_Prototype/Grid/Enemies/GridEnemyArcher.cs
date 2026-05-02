using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사수: 안 움직이며, 4방향 직선상에 플레이어 있으면 다음 턴 발사 라인 표시.
/// 턴 흐름:
///   1. 이전 턴 발사 라인에 플레이어 있으면 데미지
///   2. 직선상에 플레이어 있으면 라인 전체 텔레그래프 표시 (다음 턴 발사 예고)
/// </summary>
public class GridEnemyArcher : GridEnemyBase
{
    private List<Vector2Int> _pendingAttackLine;

    public override void OnTurnUpdate()
    {
        if (IsDead || Player == null) return;

        // 1. 이전 턴 라인 발동
        if (_pendingAttackLine != null && _pendingAttackLine.Contains(Player.GridPosition))
        {
            Player.TakeDamage(data.attackDamage);
        }
        _pendingAttackLine = null;
        ClearTelegraphs();

        // 2. 직선상에 플레이어 있으면 라인 표시
        var line = ComputeLineToPlayer();
        if (line != null)
        {
            _pendingAttackLine = line;
            foreach (var cell in line)
                ShowTelegraph(cell);
        }
    }

    /// <summary>
    /// 4방향 직선상에 플레이어가 있고, 사이에 벽이 없으면 라인 셀 리스트 반환. 아니면 null.
    /// </summary>
    private List<Vector2Int> ComputeLineToPlayer()
    {
        var diff = Player.GridPosition - GridPosition;

        Vector2Int dir;
        if (diff.x == 0 && diff.y != 0) dir = new Vector2Int(0, System.Math.Sign(diff.y));
        else if (diff.y == 0 && diff.x != 0) dir = new Vector2Int(System.Math.Sign(diff.x), 0);
        else return null;

        int maxRange = (int)data.visionRange;
        var result = new List<Vector2Int>();
        var current = GridPosition + dir;

        for (int step = 0; step < maxRange; step++)
        {
            if (!GridSystem.Instance.IsInBounds(current)) return null;
            if (!GridSystem.Instance.IsWalkable(current)) return null;  // 벽이 시야 차단

            result.Add(current);

            if (current == Player.GridPosition) return result;

            current += dir;
        }

        return null;
    }
}
