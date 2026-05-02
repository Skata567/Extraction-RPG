using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "60s Dungeon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string id;
    public int damage = 1;
    public int maxDurability = 5;
    public int startCount = 3;          // 시작 시 보유 개수
    public Sprite sprite;
}
