using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(InventoryHighlight))]
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] private List<ItemData> debugItems = new();
        [SerializeField] private InventoryItem itemPrefab;
        [SerializeField] private Transform canvasTransform;
        [SerializeField] private PlayerEquipment playerEquipment;
        [SerializeField] private bool enableDebugInsertKey;

        private ItemGrid _selectedGrid;
        private InventoryItem _selectedItem;
        private InventoryItem _overlapItem;
        private RectTransform _dragRect;
        private InventoryHighlight _inventoryHighlight;
        private readonly List<InventoryItem> _items = new();

        public event Action OnInventoryChanged;
        public event Action<InventoryItem> OnItemAdded;

        public IReadOnlyList<InventoryItem> Items => _items;

        public ItemGrid SelectedGrid
        {
            get => _selectedGrid;
            set
            {
                _selectedGrid = value;
                _inventoryHighlight.SetParent(value);
            }
        }

        private void Awake()
        {
            _inventoryHighlight = GetComponent<InventoryHighlight>();
            if (canvasTransform == null)
                canvasTransform = GetComponentInParent<Canvas>()?.transform;
            if (playerEquipment == null)
                playerEquipment = FindFirstObjectByType<PlayerEquipment>();
        }

        private void Update()
        {
            DragSelectedItemIcon();

            if (enableDebugInsertKey && Input.GetKeyDown(KeyCode.W))
                InsertRandomDebugItem();

            if (Input.GetMouseButtonDown(0))
                HandleLeftClick();

            if (_selectedGrid == null)
            {
                _inventoryHighlight.Show(false);
                return;
            }

            UpdateHighlight();
        }

        public bool TryAddItem(ItemData itemData)
        {
            if (_selectedGrid == null)
                _selectedGrid = FindFirstObjectByType<ItemGrid>();

            if (_selectedGrid == null || itemPrefab == null || canvasTransform == null || itemData == null)
            {
                Debug.LogError("InventoryController: 인벤토리 그리드, 아이템 프리팹, Canvas, ItemData 연결을 확인해야 합니다.");
                return false;
            }

            InventoryItem item = CreateInventoryItem(itemData);
            Vector2Int? pos = _selectedGrid.FindSpaceForItem(item);
            if (pos == null)
            {
                Destroy(item.gameObject);
                return false;
            }

            _selectedGrid.PlaceItemInternal(item, pos.Value.x, pos.Value.y);
            _items.Add(item);
            OnItemAdded?.Invoke(item);
            RaiseInventoryChanged();
            return true;
        }

        public bool TryRemoveItem(InventoryItem item)
        {
            if (item == null) return false;
            ItemGrid targetGrid = _selectedGrid != null ? _selectedGrid : FindFirstObjectByType<ItemGrid>();
            if (targetGrid == null)
            {
                Debug.LogError("InventoryController: 아이템을 제거할 인벤토리 그리드를 찾을 수 없습니다.");
                return false;
            }

            if (_selectedItem == item)
            {
                _selectedItem = null;
                _dragRect = null;
            }

            _items.Remove(item);
            targetGrid.RemoveItem(item);
            RaiseInventoryChanged();
            return true;
        }

        private InventoryItem CreateInventoryItem(ItemData itemData)
        {
            InventoryItem item = Instantiate(itemPrefab, canvasTransform);
            item.Set(itemData, this, playerEquipment);
            return item;
        }

        private void HandleLeftClick()
        {
            if (_selectedGrid == null) return;

            Vector2Int gridPos = GetMouseGridPosition();
            if (_selectedItem == null) TryPickUpItem(gridPos);
            else TryPlaceSelectedItem(gridPos);
        }

        private void TryPickUpItem(Vector2Int gridPos)
        {
            _selectedItem = _selectedGrid.PickUpItem(gridPos.x, gridPos.y);
            if (_selectedItem == null) return;

            _dragRect = _selectedItem.GetComponent<RectTransform>();
            _dragRect.SetParent(canvasTransform, true);
            _dragRect.SetAsLastSibling();
        }

        private void TryPlaceSelectedItem(Vector2Int gridPos)
        {
            bool placed = _selectedGrid.TryPlaceItem(_selectedItem, gridPos.x, gridPos.y, ref _overlapItem);
            if (!placed) return;

            _selectedItem = null;
            if (_overlapItem != null)
            {
                _selectedItem = _overlapItem;
                _overlapItem = null;
                _dragRect = _selectedItem.GetComponent<RectTransform>();
                _dragRect.SetParent(canvasTransform, true);
                _dragRect.SetAsLastSibling();
            }

            RaiseInventoryChanged();
        }

        private Vector2Int GetMouseGridPosition()
        {
            Vector2 mousePos = Input.mousePosition;
            if (_selectedItem != null)
            {
                mousePos.x -= (_selectedItem.ItemData.Width - 1) * ItemGrid.TileWidth / 2f;
                mousePos.y += (_selectedItem.ItemData.Height - 1) * ItemGrid.TileHeight / 2f;
            }
            return _selectedGrid.ScreenToGridPosition(mousePos);
        }

        private void DragSelectedItemIcon()
        {
            if (_selectedItem != null && _dragRect != null)
                _dragRect.position = Input.mousePosition;
        }

        private void InsertRandomDebugItem()
        {
            if (debugItems.Count == 0) return;
            TryAddItem(debugItems[UnityEngine.Random.Range(0, debugItems.Count)]);
        }

        private void UpdateHighlight()
        {
            Vector2Int gridPos = GetMouseGridPosition();
            if (_selectedItem == null)
            {
                InventoryItem target = _selectedGrid.GetItemAt(gridPos.x, gridPos.y);
                if (target == null)
                {
                    _inventoryHighlight.Show(false);
                    return;
                }

                _inventoryHighlight.Show(true);
                _inventoryHighlight.UpdateSize(target);
                _inventoryHighlight.UpdatePosition(_selectedGrid.GetLocalPositionForItem(target, target.OnGridPositionX, target.OnGridPositionY));
            }
            else
            {
                bool valid = _selectedGrid.IsWithinBoundary(gridPos.x, gridPos.y, _selectedItem.ItemData.Width, _selectedItem.ItemData.Height);
                _inventoryHighlight.Show(valid);
                _inventoryHighlight.UpdateSize(_selectedItem);
                _inventoryHighlight.UpdatePosition(_selectedGrid.GetLocalPositionForItem(_selectedItem, gridPos.x, gridPos.y));
            }
        }

        private void RaiseInventoryChanged()
        {
            OnInventoryChanged?.Invoke();
            PrototypeRTEvents.RaiseInventoryChanged();
        }
    }
}
