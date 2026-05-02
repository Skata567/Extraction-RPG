using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 행동 → 적 턴 처리 흐름 관리.
/// 적/함정은 Register()로 등록, 사망 시 Unregister().
/// </summary>
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    // 적 턴 사이 딜레이 (애니메이션 여유 시간)
    [SerializeField] private float enemyTurnDelay = 0.05f;

    private readonly List<IGridEntity> _entities = new();
    private bool _processingTurn;

    public bool IsPlayerTurn { get; private set; } = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("TurnManager가 씬에 두 개 이상 있습니다.");
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Register(IGridEntity entity)
    {
        if (!_entities.Contains(entity))
            _entities.Add(entity);
    }

    public void Unregister(IGridEntity entity)
    {
        _entities.Remove(entity);
    }

    /// <summary>
    /// 해당 격자 셀에 등록된 엔티티 반환 (단검 충돌 판정 등에 사용). 없으면 null.
    /// </summary>
    public IGridEntity GetEntityAt(Vector2Int pos)
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            var e = _entities[i];
            if (e != null && e.GridPosition == pos) return e;
        }
        return null;
    }

    /// <summary>
    /// 플레이어 행동 완료 시 호출. 시간 소모 후 적 턴 시작.
    /// </summary>
    public void OnPlayerActionCompleted(float timeCost)
    {
        if (_processingTurn) return;

        TimeSystem.Instance.ConsumeTime(timeCost);

        // 시간 소진으로 이미 게임오버 처리된 경우 적 턴 불필요
        if (!TimeSystem.Instance.IsRunning) return;

        StartCoroutine(ProcessEnemyTurns());
    }

    private IEnumerator ProcessEnemyTurns()
    {
        _processingTurn = true;
        IsPlayerTurn = false;

        // 리스트 복사 — 턴 도중 Unregister가 일어나도 안전
        var snapshot = new List<IGridEntity>(_entities);
        foreach (var entity in snapshot)
        {
            if (entity == null) continue;
            entity.OnTurnUpdate();
            if (enemyTurnDelay > 0f)
                yield return new WaitForSeconds(enemyTurnDelay);
        }

        // 죽은 엔티티 정리
        _entities.RemoveAll(e => e == null);

        _processingTurn = false;
        IsPlayerTurn = true;
    }
}
