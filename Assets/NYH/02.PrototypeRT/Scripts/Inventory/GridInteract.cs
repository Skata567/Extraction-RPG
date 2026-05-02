using UnityEngine;
using UnityEngine.EventSystems;

namespace PrototypeRT
{
    [RequireComponent(typeof(ItemGrid))]
    public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private InventoryController inventoryController;
        private ItemGrid _itemGrid;

        private void Awake()
        {
            _itemGrid = GetComponent<ItemGrid>();
            if (inventoryController == null)
                inventoryController = FindFirstObjectByType<InventoryController>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (inventoryController != null) inventoryController.SelectedGrid = _itemGrid;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (inventoryController != null) inventoryController.SelectedGrid = null;
        }
    }
}
