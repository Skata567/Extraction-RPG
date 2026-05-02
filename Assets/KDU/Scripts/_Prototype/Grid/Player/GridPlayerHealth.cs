using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어 HP + 회복 포션.
/// 포션: 즉시 +50% MaxHp, 이후 25초에 걸쳐 추가 +50% (curve로 곡선 조절).
/// </summary>
public class GridPlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int startPotionCount = 1;

    [Header("포션 회복 곡선")]
    [Tooltip("X: 0~1 진행률 (0=사용 직후, 1=25초 후), Y: 0~1 누적 회복량 비율")]
    [SerializeField] private AnimationCurve potionRegenCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.12f, 0.6f),   // ~3초까지 60% 누적
        new Keyframe(0.4f, 0.85f),   // ~10초까지 85%
        new Keyframe(1f, 1f));        // 25초에 100%
    [SerializeField] private float potionDuration = 25f;

    private GridPlayer _player;
    private Coroutine _regenCoroutine;

    public int CurrentHp { get; private set; }
    public int MaxHp => maxHp;
    public bool IsDead => CurrentHp <= 0;
    public int PotionCount { get; private set; }

    public void Init(GridPlayer player)
    {
        _player = player;
        CurrentHp = maxHp;
        PotionCount = startPotionCount;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;

        CurrentHp = Mathf.Max(0, CurrentHp - amount);
        GameEvents.OnPlayerDamaged?.Invoke(amount);

        if (CurrentHp <= 0)
        {
            GameEvents.OnPlayerDied?.Invoke();
            TimeSystem.Instance.StopTimer();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;
        CurrentHp = Mathf.Min(maxHp, CurrentHp + amount);
    }

    public void AddPotion(int count = 1)
    {
        PotionCount += count;
    }

    public void TryUsePotion()
    {
        if (PotionCount <= 0 || IsDead) return;

        PotionCount--;

        // 즉시 +50% MaxHp
        Heal(Mathf.RoundToInt(maxHp * 0.5f));

        // 진행 중인 회복 코루틴 취소 후 새로 시작
        if (_regenCoroutine != null) StopCoroutine(_regenCoroutine);
        _regenCoroutine = StartCoroutine(RegenOverTime());

        TurnManager.Instance.OnPlayerActionCompleted(_player.Config.potionTimeCost);
    }

    private IEnumerator RegenOverTime()
    {
        float elapsed = 0f;
        float pool = maxHp * 0.5f;  // 시간에 걸쳐 회복할 총량 = 50% MaxHp
        float deliveredFraction = 0f;

        while (elapsed < potionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / potionDuration);
            float currentFraction = Mathf.Clamp01(potionRegenCurve.Evaluate(progress));

            float deltaFraction = currentFraction - deliveredFraction;
            if (deltaFraction > 0f)
            {
                int delta = Mathf.RoundToInt(pool * deltaFraction);
                if (delta > 0)
                {
                    Heal(delta);
                    deliveredFraction += (float)delta / pool;
                }
            }

            if (IsDead) yield break;
            yield return null;
        }

        _regenCoroutine = null;
    }
}
