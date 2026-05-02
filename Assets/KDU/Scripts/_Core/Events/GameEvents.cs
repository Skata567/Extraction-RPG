using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    // 타이머
    public static Action<float> OnTimeChanged;
    public static Action OnTimeUp;

    // 전투
    public static Action<Vector2Int, float> OnEnemyKilled;  // pos, timeBonus
    public static Action<int> OnPlayerDamaged;              // damage amount
    public static Action<Vector2Int> OnTrapTriggered;

    // 시야
    public static Action<IReadOnlyCollection<Vector2Int>, IReadOnlyCollection<Vector2Int>> OnVisionUpdated; // visible, explored

    // 게임 상태
    public static Action OnPlayerDied;
    public static Action OnLevelComplete;

    // 단검
    public static Action<Vector2Int, Vector2Int> OnDaggerThrown;    // from, to
    public static Action<Vector2Int> OnDaggerRetrieved;

    // 씬 재시작 시 호출 — static 이벤트는 씬 전환으로 안 정리되므로 직접 초기화
    public static void ClearAllEvents()
    {
        OnTimeChanged = null;
        OnTimeUp = null;
        OnEnemyKilled = null;
        OnPlayerDamaged = null;
        OnTrapTriggered = null;
        OnVisionUpdated = null;
        OnPlayerDied = null;
        OnLevelComplete = null;
        OnDaggerThrown = null;
        OnDaggerRetrieved = null;
    }
}
