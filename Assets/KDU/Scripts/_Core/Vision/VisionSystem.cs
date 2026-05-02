using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 중심 원형 시야 + 브레젠험 직선 차단 계산.
/// 렌더링(안개 타일맵)은 구독자(FogRenderer 등)가 OnVisionUpdated 이벤트로 처리.
/// </summary>
public class VisionSystem : MonoBehaviour
{
    public static VisionSystem Instance { get; private set; }

    [SerializeField] private GameConfig config;

    private readonly HashSet<Vector2Int> _visible = new();
    private readonly HashSet<Vector2Int> _explored = new();

    public IReadOnlyCollection<Vector2Int> Visible => _visible;
    public IReadOnlyCollection<Vector2Int> Explored => _explored;

    private int VisionRadius => config != null ? config.visionRadius : 5;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("VisionSystem이 씬에 두 개 이상 있습니다.");
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// 플레이어 이동 후 호출. 시야 갱신 및 이벤트 발행.
    /// </summary>
    public void UpdateVision(Vector2Int playerPos)
    {
        _visible.Clear();

        int r = VisionRadius;
        int r2 = r * r;

        for (int dx = -r; dx <= r; dx++)
        {
            for (int dy = -r; dy <= r; dy++)
            {
                if (dx * dx + dy * dy > r2) continue;

                var cell = new Vector2Int(playerPos.x + dx, playerPos.y + dy);
                if (!GridSystem.Instance.IsInBounds(cell)) continue;
                if (!HasLineOfSight(playerPos, cell)) continue;

                _visible.Add(cell);
                _explored.Add(cell);
            }
        }

        GameEvents.OnVisionUpdated?.Invoke(_visible, _explored);
    }

    public bool IsVisible(Vector2Int pos) => _visible.Contains(pos);
    public bool IsExplored(Vector2Int pos) => _explored.Contains(pos);

    /// <summary>
    /// 브레젠험 직선 LOS 체크.
    /// 중간 경로에 벽(비통행 셀)이 있으면 차단. 목적지 셀 자체는 허용(벽도 보임).
    /// </summary>
    private bool HasLineOfSight(Vector2Int from, Vector2Int to)
    {
        int x = from.x, y = from.y;
        int dx = Mathf.Abs(to.x - from.x), dy = Mathf.Abs(to.y - from.y);
        int sx = from.x < to.x ? 1 : -1;
        int sy = from.y < to.y ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x == to.x && y == to.y) return true;

            // 출발지 제외, 중간 경로에 비통행 셀이 있으면 차단
            if (!(x == from.x && y == from.y) && !GridSystem.Instance.IsWalkable(new Vector2Int(x, y)))
                return false;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }
        }
    }
}
