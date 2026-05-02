using UnityEngine;

/// <summary>
/// 던져진 후 맵에 떨어진 단검. 시야 밖이면 안 보이고, 플레이어가 그 칸 밟으면 회수됨.
/// TurnManager에 등록하지 않음 (턴 행동 없음).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class GridDagger : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    public int Durability { get; private set; }

    private SpriteRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        GameEvents.OnVisionUpdated += OnVisionUpdated;
    }

    private void OnDisable()
    {
        GameEvents.OnVisionUpdated -= OnVisionUpdated;
    }

    public void Place(Vector2Int pos, int durability)
    {
        GridPosition = pos;
        Durability = durability;
        transform.position = GridSystem.Instance.GridToWorld(pos);

        // 배치 직후 시야 상태에 맞춰 가시성 갱신
        if (VisionSystem.Instance != null)
            _renderer.enabled = VisionSystem.Instance.IsVisible(pos);
    }

    private void OnVisionUpdated(System.Collections.Generic.IReadOnlyCollection<Vector2Int> visible,
                                  System.Collections.Generic.IReadOnlyCollection<Vector2Int> explored)
    {
        _renderer.enabled = VisionSystem.Instance.IsVisible(GridPosition);
    }
}
