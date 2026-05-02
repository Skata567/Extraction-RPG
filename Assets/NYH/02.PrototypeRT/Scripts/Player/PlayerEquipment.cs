using UnityEngine;

namespace PrototypeRT
{
    public class PlayerEquipment : MonoBehaviour
    {
        [Header("현재 장착 무기")]
        [Tooltip("플레이어가 현재 장착한 무기 데이터입니다. 테스트용으로 미리 무기를 들고 시작하고 싶을 때만 넣습니다.")]
        [SerializeField] private ItemData equippedWeapon;

        [Header("무기 표시")]
        [Tooltip("장착한 무기 이미지를 화면에 보여줄 SpriteRenderer입니다. 플레이어 자식 오브젝트의 WeaponVisual SpriteRenderer를 넣어주세요.")]
        [SerializeField] private SpriteRenderer weaponRenderer;

        public ItemData EquippedWeapon => equippedWeapon;
        public int AttackBonus => equippedWeapon != null ? equippedWeapon.AttackBonus : 0;

        public bool TryEquip(ItemData itemData)
        {
            if (itemData == null) return false;
            if (itemData.ItemType != ItemType.Weapon || itemData.EquipmentSlot != EquipmentSlot.Weapon)
            {
                Debug.Log($"{itemData.ItemName}은 무기 슬롯에 장착할 수 없습니다.");
                return false;
            }

            equippedWeapon = itemData;
            ApplyWeaponVisual(itemData);
            PrototypeRTEvents.RaiseWeaponEquipped(itemData);
            return true;
        }

        private void ApplyWeaponVisual(ItemData itemData)
        {
            if (weaponRenderer == null)
            {
                Debug.LogWarning("PlayerEquipment: Weapon Renderer가 비어 있습니다. 장착은 되었지만 손에 든 무기 이미지는 바뀌지 않습니다.");
                return;
            }

            weaponRenderer.sprite = itemData.GetWorldSprite();
            weaponRenderer.enabled = weaponRenderer.sprite != null;
        }
    }
}
