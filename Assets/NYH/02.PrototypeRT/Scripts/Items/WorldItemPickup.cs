using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(Collider2D))]
    public class WorldItemPickup : MonoBehaviour, IInteractable
    {
        [Header("아이템 데이터")]
        [Tooltip("이 픽업 오브젝트가 플레이어에게 줄 아이템입니다. 드랍으로 생성될 때는 ItemDropper가 자동으로 넣어줍니다.")]
        [SerializeField] private ItemData itemData;

        [Header("월드 표시")]
        [Tooltip("월드에 떨어진 아이템 이미지를 보여줄 SpriteRenderer입니다. 비워두면 자식 포함해서 자동으로 찾습니다.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("상호작용 범위")]
        [Tooltip("플레이어가 이 거리 안에 있을 때 E 키로 아이템을 주울 수 있습니다.")]
        [SerializeField, Min(0f)] private float pickupRadius = 1.2f;

        private InventoryController _inventoryController;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;
            ApplyVisual();
        }

        private void Start()
        {
            _inventoryController = FindFirstObjectByType<InventoryController>();
        }

        public void SetItem(ItemData newItemData)
        {
            itemData = newItemData;
            ApplyVisual();
        }

        public bool CanInteract(GameObject interactor)
        {
            return itemData != null && Vector2.Distance(transform.position, interactor.transform.position) <= pickupRadius;
        }

        public void Interact(GameObject interactor)
        {
            if (_inventoryController == null)
                _inventoryController = FindFirstObjectByType<InventoryController>();

            if (_inventoryController == null)
            {
                Debug.LogError("WorldItemPickup: InventoryController가 씬에 없습니다.");
                return;
            }

            // 공간이 없으면 필드 아이템을 남겨두어 전리품 손실이 즉시 일어나지 않게 한다.
            if (!_inventoryController.TryAddItem(itemData)) return;

            PrototypeRTEvents.RaiseItemPickedUp(itemData);
            Destroy(gameObject);
        }

        private void ApplyVisual()
        {
            if (spriteRenderer != null && itemData != null)
                spriteRenderer.sprite = itemData.ItemIcon;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}
