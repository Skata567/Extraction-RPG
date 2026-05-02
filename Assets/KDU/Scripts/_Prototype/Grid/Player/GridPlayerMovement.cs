using UnityEngine;

/// <summary>
/// 4방향 격자 이동 + facing 추적. 차단 시 facing만 갱신, 시간 소모 없음(자유 회전).
/// </summary>
public class GridPlayerMovement : MonoBehaviour
{
    public Vector2Int Facing { get; private set; } = Vector2Int.right;

    private GridPlayer _player;

    public void Init(GridPlayer player)
    {
        _player = player;
    }

    public void TryMove(Vector2Int direction)
    {
        // 회전은 항상 갱신 (차단되어도 방향은 바뀜 — 던지기 방향 잡기 용도)
        Facing = direction;

        var target = _player.GridPosition + direction;

        if (!GridSystem.Instance.IsWalkable(target))
        {
            // 벽에 막혔지만 회전만 한 셈 — 시간 소모 없음
            return;
        }

        // 이동 후 단검 회수 시도 (이동 비용 + 회수 비용 합산)
        float totalCost = _player.Config.moveTimeCost;
        if (_player.Combat.TryRetrieveAt(target))
        {
            totalCost += _player.Config.retrieveTimeCost;
        }

        _player.SetGridPosition(target);
        VisionSystem.Instance.UpdateVision(target);

        TurnManager.Instance.OnPlayerActionCompleted(totalCost);
    }
}
