using UnityEngine;

namespace PrototypeRT
{
    public class InventoryHighlight : MonoBehaviour
    {
        [SerializeField] private RectTransform highlight;

        public void Show(bool value)
        {
            if (highlight != null) highlight.gameObject.SetActive(value);
        }

        public void UpdateSize(InventoryItem item)
        {
            if (highlight == null || item == null || item.ItemData == null) return;
            highlight.sizeDelta = new Vector2(item.ItemData.Width * ItemGrid.TileWidth, item.ItemData.Height * ItemGrid.TileHeight);
        }

        public void UpdatePosition(Vector2 localPos)
        {
            if (highlight != null) highlight.localPosition = localPos;
        }

        public void SetParent(ItemGrid grid)
        {
            if (highlight == null || grid == null) return;
            highlight.SetParent(grid.GetComponent<RectTransform>(), false);
            highlight.SetAsLastSibling();
        }
    }
}
