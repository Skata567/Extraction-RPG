using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 단검 인벤토리 + 던지기/회수. 던진 단검은 GridDagger 엔티티로 맵에 남음.
/// 회수는 GridPlayerMovement가 이동 시 TryRetrieveAt 호출.
/// </summary>
public class GridPlayerCombat : MonoBehaviour
{
    [SerializeField] private WeaponData daggerData;
    [SerializeField] private GridDagger daggerPrefab;
    [SerializeField] private int maxThrowDistance = 10;

    // 인벤토리: 각 단검의 현재 내구도 리스트
    private readonly List<int> _inventory = new();

    // 맵에 떨어진 단검 위치 → 단검 엔티티 매핑
    private readonly Dictionary<Vector2Int, GridDagger> _thrownDaggers = new();

    private GridPlayer _player;

    public int InventoryCount => _inventory.Count;
    public IReadOnlyList<int> InventoryDurabilities => _inventory;

    public void Init(GridPlayer player)
    {
        _player = player;

        if (daggerData == null)
        {
            Debug.LogError("GridPlayerCombat: WeaponData(dagger)가 할당되지 않았습니다.");
            return;
        }

        // 시작 단검 채우기 (durability 모두 max)
        for (int i = 0; i < daggerData.startCount; i++)
            _inventory.Add(daggerData.maxDurability);
    }

    public void TryThrow()
    {
        if (_inventory.Count == 0) return;

        var dir = _player.Movement.Facing;
        var origin = _player.GridPosition;

        if (!ResolveTrajectory(origin, dir, out var landingCell, out var hitTarget))
        {
            // 첫 셀이 차단됨 — 던지기 자체가 무효, 시간 소모 없음
            return;
        }

        // 인벤토리에서 단검 1개 꺼냄 (현재 내구도 보존)
        int durability = _inventory[0];
        _inventory.RemoveAt(0);

        // 적중했으면 데미지
        if (hitTarget != null)
        {
            hitTarget.TakeDamage(daggerData.damage);
        }

        // 맵에 단검 엔티티 생성
        if (daggerPrefab != null)
        {
            var dagger = Instantiate(daggerPrefab);
            dagger.Place(landingCell, durability);
            _thrownDaggers[landingCell] = dagger;
        }

        GameEvents.OnDaggerThrown?.Invoke(origin, landingCell);

        TurnManager.Instance.OnPlayerActionCompleted(_player.Config.throwTimeCost);
    }

    /// <summary>
    /// 이동 시 도착 칸에 단검이 있으면 회수. 회수 성공 시 true.
    /// 내구도 -1, 0 이하면 사라짐. 호출자가 추가 시간 비용 합산.
    /// </summary>
    public bool TryRetrieveAt(Vector2Int cell)
    {
        if (!_thrownDaggers.TryGetValue(cell, out var dagger)) return false;

        int newDurability = dagger.Durability - 1;
        _thrownDaggers.Remove(cell);
        Destroy(dagger.gameObject);

        if (newDurability > 0)
        {
            _inventory.Add(newDurability);
        }

        GameEvents.OnDaggerRetrieved?.Invoke(cell);
        return true;
    }

    /// <summary>
    /// origin에서 dir 방향으로 직선 이동하며 단검 궤적 계산.
    /// landingCell: 단검이 떨어질 셀, hitTarget: 적중한 IDamageable (없으면 null).
    /// 첫 셀부터 차단되면 false 반환 (던지기 무효).
    /// </summary>
    private bool ResolveTrajectory(Vector2Int origin, Vector2Int dir, out Vector2Int landingCell, out IDamageable hitTarget)
    {
        landingCell = origin;
        hitTarget = null;

        var current = origin + dir;

        // 첫 셀이 벽이면 무효
        if (!GridSystem.Instance.IsInBounds(current) || !GridSystem.Instance.IsWalkable(current))
            return false;

        for (int step = 0; step < maxThrowDistance; step++)
        {
            if (!GridSystem.Instance.IsInBounds(current)) break;
            if (!GridSystem.Instance.IsWalkable(current)) break;

            // 적중 검사
            var entity = TurnManager.Instance.GetEntityAt(current);
            if (entity is IDamageable damageable && (Object)damageable != _player)
            {
                hitTarget = damageable;
                landingCell = current;
                return true;
            }

            landingCell = current;
            current += dir;
        }

        return true;
    }
}
