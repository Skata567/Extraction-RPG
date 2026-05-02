using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    public static TimeSystem Instance { get; private set; }

    [SerializeField] private GameConfig config;

    public float TimeRemaining { get; private set; }
    public bool IsRunning { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("TimeSystem이 씬에 두 개 이상 있습니다.");
            return;
        }
        Instance = this;
        TimeRemaining = config != null ? config.startTime : 60f;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void StartTimer() => IsRunning = true;
    public void StopTimer() => IsRunning = false;

    private void Update()
    {
        if (!IsRunning) return;

        TimeRemaining -= Time.deltaTime;
        GameEvents.OnTimeChanged?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            IsRunning = false;
            GameEvents.OnTimeUp?.Invoke();
        }
    }

    /// <summary>
    /// 행동 실행 시 시간 소모 (이동 0.3s, 던지기 0.5s 등)
    /// </summary>
    public void ConsumeTime(float seconds)
    {
        if (!IsRunning) return;

        TimeRemaining = Mathf.Max(0f, TimeRemaining - seconds);
        GameEvents.OnTimeChanged?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            IsRunning = false;
            GameEvents.OnTimeUp?.Invoke();
        }
    }

    /// <summary>
    /// 적 처치 보너스 등 시간 추가
    /// </summary>
    public void AddTime(float seconds)
    {
        TimeRemaining += seconds;
        GameEvents.OnTimeChanged?.Invoke(TimeRemaining);
    }
}
