using UnityEngine;

namespace PrototypeRT
{
    public enum ItemType
    {
        Loot,
        Weapon,
        Consumable
    }

    public enum EquipmentSlot
    {
        None,
        Weapon
    }

    [CreateAssetMenu(fileName = "ItemData", menuName = "Prototype RT/Item Data")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string itemName = "New Item";
        [SerializeField, Min(1)] private int width = 1;
        [SerializeField, Min(1)] private int height = 1;
        [SerializeField] private Sprite itemIcon;
        [SerializeField] private Sprite weaponWorldSprite;
        [SerializeField] private ItemType itemType = ItemType.Loot;
        [SerializeField] private EquipmentSlot equipmentSlot = EquipmentSlot.None;
        [SerializeField, Min(0)] private int sellPrice = 1;
        [SerializeField] private int attackBonus;

        public string ItemName => itemName;
        public int Width => width;
        public int Height => height;
        public Sprite ItemIcon => itemIcon;
        public Sprite WeaponWorldSprite => weaponWorldSprite;
        public ItemType ItemType => itemType;
        public EquipmentSlot EquipmentSlot => equipmentSlot;
        public int SellPrice => sellPrice;
        public int AttackBonus => attackBonus;

        public Sprite GetWorldSprite()
        {
            // 인벤토리 아이콘과 손에 든 무기 이미지를 분리하되, 테스트 데이터가 덜 채워져도 바로 볼 수 있게 아이콘을 fallback으로 쓴다.
            return weaponWorldSprite != null ? weaponWorldSprite : itemIcon;
        }
    }
}
