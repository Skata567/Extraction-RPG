using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(InventoryHighlight))]
    public class InventoryController : MonoBehaviour
    {
        [Header("디버그 아이템")]
        [Tooltip("테스트 키로 인벤토리에 넣을 아이템 목록입니다. 실제 플레이 드랍과는 별개입니다.")]
        [SerializeField] private List<ItemData> debugItems = new();

        [Tooltip("W 키로 debugItems 중 하나를 인벤토리에 넣을지 정합니다. 테스트용이므로 빌드 전에는 꺼두는 것을 권장합니다.")]
        [SerializeField] private bool enableDebugInsertKey;

        [Header("UI 프리팹")]
        [Tooltip("인벤토리 칸에 생성될 아이템 UI 프리팹입니다. InventoryItem 컴포넌트와 Image가 붙은 프리팹을 넣어야 합니다.")]
        [SerializeField] private InventoryItem itemPrefab;

        [Tooltip("아이템 UI를 생성할 Canvas Transform입니다. 비워두면 부모 Canvas를 자동으로 찾습니다.")]
        [SerializeField] private Transform canvasTransform;

        [Header("플레이어 장비 연결")]
        [Tooltip("아이템 더블클릭으로 무기를 장착할 대상입니다. 비워두면 씬에서 PlayerEquipment를 자동으로 찾습니다.")]
        [SerializeField] private PlayerEquipment playerEquipment;

        private ItemGrid _selectedGrid;
        private InventoryItem _selectedItem;
        private InventoryItem _overlapItem;
        private RectTransform _dragRect;
        private InventoryHighlight _inventoryHighlight;
        private readonly List<InventoryItem> _items = new();
        private bool _isPointerInputBlocked;

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
            if (_selectedGrid == null)
                SelectedGrid = ResolveItemGrid();
        }

        private void Update()
        {
            DragSelectedItemIcon();

            if (_isPointerInputBlocked)
            {
                _inventoryHighlight.Show(false);
                return;
            }

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
                SelectedGrid = ResolveItemGrid();

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
            ItemGrid targetGrid = _selectedGrid != null ? _selectedGrid : ResolveItemGrid();
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

        public InventoryItem CreateItemView(ItemData itemData, Transform parent, bool playerOwned)
        {
            if (itemPrefab == null || itemData == null || parent == null)
            {
                Debug.LogError("InventoryController: 아이템 UI 뷰를 만들기 위한 Prefab, ItemData, Parent 연결을 확인해야 합니다.");
                return null;
            }

            InventoryItem item = Instantiate(itemPrefab, parent);
            item.Set(itemData, playerOwned ? this : null, playerOwned ? playerEquipment : null);
            return item;
        }

        public void SetPointerInputBlocked(bool value)
        {
            _isPointerInputBlocked = value;
            if (!value) return;

            _selectedItem = null;
            _overlapItem = null;
            _dragRect = null;
            _inventoryHighlight.Show(false);
        }

        private InventoryItem CreateInventoryItem(ItemData itemData)
        {
            return CreateItemView(itemData, canvasTransform, true);
        }

        private ItemGrid ResolveItemGrid()
        {
            ItemGrid activeGrid = FindFirstObjectByType<ItemGrid>();
            if (activeGrid != null) return activeGrid;

            // 인벤토리 패널이 닫힌 상태에서도 월드 아이템 획득은 가능해야 하므로 비활성 UI까지 찾는다.
            ItemGrid[] allGrids = Resources.FindObjectsOfTypeAll<ItemGrid>();
            foreach (ItemGrid grid in allGrids)
            {
                if (grid == null || !grid.gameObject.scene.IsValid()) continue;
                return grid;
            }

            return null;
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
