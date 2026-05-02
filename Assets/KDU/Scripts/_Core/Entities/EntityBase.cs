using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class EntityBase : MonoBehaviour, IGridEntity
{
    public Vector2Int GridPosition { get; protected set; }

    protected SpriteRenderer SpriteRenderer { get; private set; }

    protected virtual void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 씬 배치 후 초기 격자 위치 설정. 자식 클래스에서 base.Initialize() 호출 필수.
    /// </summary>
    public virtual void Initialize(Vector2Int startPos)
    {
        GridPosition = startPos;
        SyncWorldPosition();
    }

    /// <summary>
    /// 격자 위치 변경. 호출 전 IsWalkable 등 검증은 호출자 책임.
    /// </summary>
    public virtual void SetGridPosition(Vector2Int newPos)
    {
        GridPosition = newPos;
        SyncWorldPosition();
    }

    private void SyncWorldPosition()
    {
        transform.position = GridSystem.Instance.GridToWorld(GridPosition);
    }

    public abstract void OnTurnUpdate();
}
