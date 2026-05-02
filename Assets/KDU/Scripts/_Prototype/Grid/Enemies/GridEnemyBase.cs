using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적 공통 베이스. HP, 시야 가시성, 죽음 처리, 텔레그래프 관리.
/// 자식 클래스가 OnTurnUpdate()에서 AI 구현.
/// </summary>
public abstract class GridEnemyBase : EntityBase, IDamageable
{
    [SerializeField] protected EnemyData data;
    [SerializeField] protected TelegraphIndicator telegraphPrefab;

    public int CurrentHp { get; protected set; }
    public int MaxHp => data != null ? data.maxHp : 1;
    public bool IsDead => CurrentHp <= 0;

    protected GridPlayer Player { get; private set; }

    private readonly List<TelegraphIndicator> _activeTelegraphs = new();

    protected override void Awake()
    {
        base.Awake();
        if (data == null)
        {
            Debug.LogError($"{name}: EnemyData가 할당되지 않았습니다.");
            return;
        }

        CurrentHp = data.maxHp;
        if (data.sprite != null) SpriteRenderer.sprite = data.sprite;
    }

    private void OnEnable()
    {
        GameEvents.OnVisionUpdated += OnVisionUpdated;
    }

    private void OnDisable()
    {
        GameEvents.OnVisionUpdated -= OnVisionUpdated;
    }

    protected virtual void Start()
    {
        Player = FindFirstObjectByType<GridPlayer>();
        if (Player == null)
        {
            Debug.LogError($"{name}: 씬에 GridPlayer가 없습니다.");
            return;
        }

        // 씬 배치 위치를 격자에 스냅
        var startPos = GridSystem.Instance.WorldToGrid(transform.position);
        Initialize(startPos);

        TurnManager.Instance.Register(this);
        UpdateVisibility();
    }

    private void OnVisionUpdated(IReadOnlyCollection<Vector2Int> visible, IReadOnlyCollection<Vector2Int> explored)
    {
        UpdateVisibility();
    }

    protected void UpdateVisibility()
    {
        bool visible = VisionSystem.Instance != null && VisionSystem.Instance.IsVisible(GridPosition);
        SpriteRenderer.enabled = visible;
        foreach (var t in _activeTelegraphs)
            if (t != null) t.RefreshVisibility();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;

        CurrentHp -= amount;
        if (CurrentHp <= 0) Die();
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;
        CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
    }

    protected virtual void Die()
    {
        ClearTelegraphs();
        TurnManager.Instance.Unregister(this);
        GameEvents.OnEnemyKilled?.Invoke(GridPosition, data.timeBonus);
        TimeSystem.Instance.AddTime(data.timeBonus);
        Destroy(gameObject);
    }

    /// <summary>
    /// 셀에 공격 의도 텔레그래프 표시. 호출자가 ClearTelegraphs로 정리.
    /// </summary>
    protected void ShowTelegraph(Vector2Int cell)
    {
        if (telegraphPrefab == null) return;
        var ind = Instantiate(telegraphPrefab);
        ind.Show(cell);
        _activeTelegraphs.Add(ind);
    }

    protected void ClearTelegraphs()
    {
        foreach (var t in _activeTelegraphs)
            if (t != null) Destroy(t.gameObject);
        _activeTelegraphs.Clear();
    }

    protected bool IsAdjacentToPlayer()
    {
        var diff = Player.GridPosition - GridPosition;
        return Mathf.Abs(diff.x) + Mathf.Abs(diff.y) == 1;
    }

    protected float SquaredDistanceToPlayer()
    {
        var diff = Player.GridPosition - GridPosition;
        return diff.x * diff.x + diff.y * diff.y;
    }
}
