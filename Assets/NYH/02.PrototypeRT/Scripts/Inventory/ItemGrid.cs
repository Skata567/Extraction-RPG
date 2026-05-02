using System.Collections.Generic;
using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(RectTransform))]
    public class ItemGrid : MonoBehaviour
    {
        public const float TileWidth = 32f;
        public const float TileHeight = 32f;

        [Header("그리드 크기")]
        [Tooltip("인벤토리의 가로 칸 수입니다. 실제 UI 너비는 이 값에 칸 너비 32픽셀을 곱해 자동으로 정해집니다.")]
        [SerializeField, Min(1)] private int gridWidth = 20;

        [Tooltip("인벤토리의 세로 칸 수입니다. 실제 UI 높이는 이 값에 칸 높이 32픽셀을 곱해 자동으로 정해집니다.")]
        [SerializeField, Min(1)] private int gridHeight = 10;

        private InventoryItem[,] _slots;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            InitGrid(gridWidth, gridHeight);
        }

        private void InitGrid(int width, int height)
        {
            gridWidth = Mathf.Max(1, width);
            gridHeight = Mathf.Max(1, height);
            _slots = new InventoryItem[gridWidth, gridHeight];
            _rectTransform.sizeDelta = new Vector2(gridWidth * TileWidth, gridHeight * TileHeight);
        }

        public void ConfigureSize(int width, int height)
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            InitGrid(width, height);
        }

        public InventoryItem GetItemAt(int x, int y)
        {
            if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight) return null;
            return _slots[x, y];
        }

        public InventoryItem PickUpItem(int x, int y)
        {
            InventoryItem item = GetItemAt(x, y);
            if (item == null) return null;

            ClearItemReferences(item);
            return item;
        }

        public bool TryPlaceItem(InventoryItem item, int x, int y, ref InventoryItem overlapItem)
        {
            if (item == null || item.ItemData == null) return false;
            if (!IsWithinBoundary(x, y, item.ItemData.Width, item.ItemData.Height)) return false;
            if (!CheckOverlap(x, y, item.ItemData.Width, item.ItemData.Height, ref overlapItem))
            {
                overlapItem = null;
                return false;
            }

            if (overlapItem != null) ClearItemReferences(overlapItem);
            PlaceItemInternal(item, x, y);
            return true;
        }

        public void PlaceItemInternal(InventoryItem item, int x, int y)
        {
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.SetParent(_rectTransform, false);

            for (int ix = 0; ix < item.ItemData.Width; ix++)
            {
                for (int iy = 0; iy < item.ItemData.Height; iy++)
                    _slots[x + ix, y + iy] = item;
            }

            item.OnGridPositionX = x;
            item.OnGridPositionY = y;
            itemRect.anchoredPosition = GetLocalPositionForItem(item, x, y);
        }

        public Vector2Int ScreenToGridPosition(Vector2 screenPos)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, screenPos, null, out Vector2 local))
                return new Vector2Int(-1, -1);

            Vector2 topLeft = GetTopLeftLocalPosition();
            int x = Mathf.FloorToInt((local.x - topLeft.x) / TileWidth);
            int y = Mathf.FloorToInt((topLeft.y - local.y) / TileHeight);
            return new Vector2Int(x, y);
        }

        public Vector2 GetLocalPositionForItem(InventoryItem item, int x, int y)
        {
            Vector2 topLeft = GetTopLeftLocalPosition();
            return new Vector2(
                topLeft.x + x * TileWidth + TileWidth * item.ItemData.Width / 2f,
                topLeft.y - (y * TileHeight + TileHeight * item.ItemData.Height / 2f));
        }

        private Vector2 GetTopLeftLocalPosition()
        {
            Rect rect = _rectTransform.rect;
            return new Vector2(
                -rect.width * _rectTransform.pivot.x,
                rect.height * (1f - _rectTransform.pivot.y));
        }

        public bool IsWithinBoundary(int x, int y, int width, int height)
        {
            return x >= 0 && y >= 0 && x + width <= gridWidth && y + height <= gridHeight;
        }

        public Vector2Int? FindSpaceForItem(InventoryItem item)
        {
            for (int y = 0; y <= gridHeight - item.ItemData.Height; y++)
            {
                for (int x = 0; x <= gridWidth - item.ItemData.Width; x++)
                {
                    if (IsAreaEmpty(x, y, item.ItemData.Width, item.ItemData.Height))
                        return new Vector2Int(x, y);
                }
            }
            return null;
        }

        public void RemoveItem(InventoryItem item)
        {
            if (item == null) return;
            ClearItemReferences(item);
            Destroy(item.gameObject);
        }

        public void ClearAllItems(bool destroyItems)
        {
            if (_slots == null) return;

            if (destroyItems)
            {
                HashSet<InventoryItem> uniqueItems = new();
                foreach (InventoryItem item in _slots)
                {
                    if (item != null)
                        uniqueItems.Add(item);
                }

                foreach (InventoryItem item in uniqueItems)
                {
                    if (item != null)
                        Destroy(item.gameObject);
                }
            }

            _slots = new InventoryItem[gridWidth, gridHeight];
        }

        private bool CheckOverlap(int x, int y, int width, int height, ref InventoryItem overlap)
        {
            for (int ix = 0; ix < width; ix++)
            {
                for (int iy = 0; iy < height; iy++)
                {
                    InventoryItem current = _slots[x + ix, y + iy];
                    if (current == null) continue;
                    if (overlap == null) overlap = current;
                    else if (overlap != current) return false;
                }
            }
            return true;
        }

        private bool IsAreaEmpty(int x, int y, int width, int height)
        {
            for (int ix = 0; ix < width; ix++)
            {
                for (int iy = 0; iy < height; iy++)
                {
                    if (_slots[x + ix, y + iy] != null) return false;
                }
            }
            return true;
        }

        private void ClearItemReferences(InventoryItem item)
        {
            if (item.ItemData == null) return;

            for (int ix = 0; ix < item.ItemData.Width; ix++)
            {
                for (int iy = 0; iy < item.ItemData.Height; iy++)
                {
                    int x = item.OnGridPositionX + ix;
                    int y = item.OnGridPositionY + iy;
                    if (x >= 0 && y >= 0 && x < gridWidth && y < gridHeight && _slots[x, y] == item)
                        _slots[x, y] = null;
                }
            }
        }
    }
}
