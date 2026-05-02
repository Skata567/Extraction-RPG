using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 시야 안개 렌더러. VisionSystem 이벤트 구독 → 안개 타일맵 갱신.
/// 셀 상태:
///   - 안 본 곳: fogTile (불투명 어둠)
///   - 본 적 있음 + 현재 안 보임: exploredTile (반투명 어둠)
///   - 현재 보임: 타일 없음 (투명)
/// </summary>
public class FogRenderer : MonoBehaviour
{
    [SerializeField] private Tilemap fogTilemap;
    [SerializeField] private TileBase fogTile;
    [SerializeField] private TileBase exploredTile;

    private bool _initialized;

    private void OnEnable()
    {
        GameEvents.OnVisionUpdated += OnVisionUpdated;
    }

    private void OnDisable()
    {
        GameEvents.OnVisionUpdated -= OnVisionUpdated;
    }

    private void Start()
    {
        FillInitialFog();
    }

    private void FillInitialFog()
    {
        if (fogTilemap == null || fogTile == null || GridSystem.Instance == null) return;

        foreach (var cell in GridSystem.Instance.GetAllCells())
        {
            fogTilemap.SetTile(new Vector3Int(cell.x, cell.y, 0), fogTile);
        }
        _initialized = true;
    }

    private void OnVisionUpdated(IReadOnlyCollection<Vector2Int> visible, IReadOnlyCollection<Vector2Int> explored)
    {
        if (!_initialized) FillInitialFog();
        if (fogTilemap == null) return;

        // 탐험된 셀들만 갱신: 현재 보이면 투명, 아니면 반투명
        foreach (var cell in explored)
        {
            var pos = new Vector3Int(cell.x, cell.y, 0);
            fogTilemap.SetTile(pos, VisionSystem.Instance.IsVisible(cell) ? null : exploredTile);
        }
    }
}
