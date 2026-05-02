using UnityEngine;
using UnityEngine.Tilemaps;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance { get; private set; }

    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("GridSystem이 씬에 두 개 이상 있습니다.");
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// 격자 좌표 → 월드 좌표 (셀 중심점)
    /// </summary>
    public Vector3 GridToWorld(Vector2Int pos)
    {
        return floorTilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
    }

    /// <summary>
    /// 월드 좌표 → 격자 좌표
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3Int cell = floorTilemap.WorldToCell(worldPos);
        return new Vector2Int(cell.x, cell.y);
    }

    /// <summary>
    /// 해당 격자 셀이 이동 가능한지 (바닥 타일 있고 벽 타일 없음)
    /// </summary>
    public bool IsWalkable(Vector2Int pos)
    {
        var cell = new Vector3Int(pos.x, pos.y, 0);
        if (wallTilemap != null && wallTilemap.HasTile(cell)) return false;
        return floorTilemap.HasTile(cell);
    }

    /// <summary>
    /// 맵 범위 안인지 (바닥 또는 벽 타일이 존재하는 셀)
    /// </summary>
    public bool IsInBounds(Vector2Int pos)
    {
        var cell = new Vector3Int(pos.x, pos.y, 0);
        if (floorTilemap.HasTile(cell)) return true;
        return wallTilemap != null && wallTilemap.HasTile(cell);
    }

    /// <summary>
    /// 맵에 존재하는 모든 통행 가능 셀 반환 (VisionSystem 초기화 등에 사용)
    /// </summary>
    public System.Collections.Generic.IEnumerable<Vector2Int> GetAllWalkableCells()
    {
        foreach (var pos in floorTilemap.cellBounds.allPositionsWithin)
        {
            if (floorTilemap.HasTile(pos))
            {
                var grid = new Vector2Int(pos.x, pos.y);
                if (IsWalkable(grid))
                    yield return grid;
            }
        }
    }

    /// <summary>
    /// 맵에 존재하는 모든 셀 반환 (바닥 + 벽 포함, 안개 초기화 등에 사용)
    /// </summary>
    public System.Collections.Generic.IEnumerable<Vector2Int> GetAllCells()
    {
        var visited = new System.Collections.Generic.HashSet<Vector2Int>();

        foreach (var pos in floorTilemap.cellBounds.allPositionsWithin)
        {
            if (floorTilemap.HasTile(pos))
                visited.Add(new Vector2Int(pos.x, pos.y));
        }

        if (wallTilemap != null)
        {
            foreach (var pos in wallTilemap.cellBounds.allPositionsWithin)
            {
                if (wallTilemap.HasTile(pos))
                    visited.Add(new Vector2Int(pos.x, pos.y));
            }
        }

        return visited;
    }
}
