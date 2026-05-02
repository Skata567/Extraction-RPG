using UnityEngine;

public interface IGridEntity
{
    Vector2Int GridPosition { get; }
    void OnTurnUpdate();
}
