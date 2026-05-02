using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PrototypeRT
{
    [RequireComponent(typeof(Image))]
    public class InventoryItem : MonoBehaviour, IPointerClickHandler
    {
        public ItemData ItemData { get; private set; }
        public int OnGridPositionX { get; set; }
        public int OnGridPositionY { get; set; }

        private Image _image;
        private PlayerEquipment _equipment;
        private InventoryController _inventoryController;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void Set(ItemData itemData, InventoryController inventoryController, PlayerEquipment equipment)
        {
            ItemData = itemData;
            _inventoryController = inventoryController;
            _equipment = equipment;

            if (_image == null) _image = GetComponent<Image>();
            _image.sprite = itemData.ItemIcon;
            _image.enabled = itemData.ItemIcon != null;
            _image.raycastTarget = true;

            // 그리드 크기와 UI 크기를 데이터에서 계산하면, 아이템 모양을 늘려도 배치 로직은 그대로 유지된다.
            var rect = GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(itemData.Width * ItemGrid.TileWidth, itemData.Height * ItemGrid.TileHeight);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount < 2 || ItemData == null || _equipment == null) return;

            // v0.1에서는 더블클릭 장착만 지원한다. 장비 슬롯 드롭 UI는 이후 확장 지점으로 남긴다.
            if (_equipment.TryEquip(ItemData))
                StartCoroutine(RemoveAfterPointerEvent());
        }

        private IEnumerator RemoveAfterPointerEvent()
        {
            // 클릭 이벤트 처리 중 UI 오브젝트를 바로 제거하면 Unity 에디터 GUI assertion이 발생할 수 있어 한 프레임 늦춘다.
            yield return null;
            _inventoryController.TryRemoveItem(this);
        }
    }
}
