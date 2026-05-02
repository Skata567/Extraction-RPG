using UnityEngine;

public enum AttackPattern { Melee, Ranged }

[CreateAssetMenu(fileName = "EnemyData", menuName = "60s Dungeon/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string id;
    public int maxHp = 1;
    public int attackDamage = 10;
    public float timeBonus = 3f;        // 처치 시 추가 시간 (초)
    public AttackPattern attackPattern;
    public float visionRange = 6f;      // 플레이어 감지 거리
    public Sprite sprite;
}
