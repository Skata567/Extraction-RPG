using UnityEngine;

public enum TrapType { Spike, Alarm }

[CreateAssetMenu(fileName = "TrapData", menuName = "60s Dungeon/Trap Data")]
public class TrapData : ScriptableObject
{
    public string id;
    public TrapType trapType;
    public int damage = 10;
    public bool startsHidden;
    [Range(0f, 1f)] public float revealChance = 0.3f;  // 인접 시 매 턴 공개 확률
    public int alarmDuration = 10;                       // 경고 덫 발동 시 추적 지속 턴 수
    public Sprite visibleSprite;
    public Sprite hiddenSprite;
}
