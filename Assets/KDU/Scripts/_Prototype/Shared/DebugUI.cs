using TMPro;
using UnityEngine;

/// <summary>
/// 프로토타입용 디버그 UI. 시간/HP/단검/포션/상태 표시.
/// 풀 폴리시는 본 개발에서.
/// </summary>
public class DebugUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text daggerText;
    [SerializeField] private TMP_Text potionText;
    [SerializeField] private TMP_Text statusText;

    private GridPlayer _player;

    private void OnEnable()
    {
        GameEvents.OnPlayerDied += OnPlayerDied;
        GameEvents.OnTimeUp += OnTimeUp;
        GameEvents.OnLevelComplete += OnLevelComplete;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDied -= OnPlayerDied;
        GameEvents.OnTimeUp -= OnTimeUp;
        GameEvents.OnLevelComplete -= OnLevelComplete;
    }

    private void Start()
    {
        _player = FindFirstObjectByType<GridPlayer>();
        if (statusText != null) statusText.text = "";
    }

    private void Update()
    {
        if (timeText != null && TimeSystem.Instance != null)
            timeText.text = $"TIME {TimeSystem.Instance.TimeRemaining:0.0}s";

        if (_player == null) return;

        if (hpText != null)
            hpText.text = $"HP {_player.CurrentHp}/{_player.MaxHp}";

        if (daggerText != null)
        {
            var durabilities = _player.Combat.InventoryDurabilities;
            daggerText.text = durabilities.Count == 0
                ? "단검 0"
                : $"단검 {durabilities.Count} [{string.Join(",", durabilities)}]";
        }

        if (potionText != null)
            potionText.text = $"포션 {_player.Health.PotionCount}";
    }

    private void OnPlayerDied()    => SetStatus("사망 - 재시작 중...");
    private void OnTimeUp()        => SetStatus("시간 초과 - 재시작 중...");
    private void OnLevelComplete() => SetStatus("클리어! - 재시작 중...");

    private void SetStatus(string text)
    {
        if (statusText != null) statusText.text = text;
    }
}
