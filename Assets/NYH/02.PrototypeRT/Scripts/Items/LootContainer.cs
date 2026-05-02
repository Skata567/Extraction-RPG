using System.Collections.Generic;
using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(Collider2D))]
    public class LootContainer : MonoBehaviour, IInteractable, IProgressInteractable
    {
        [Header("루팅 진행")]
        [Tooltip("주머니나 상자를 수색하는 데 필요한 시간입니다.")]
        [SerializeField, Min(0.1f)] private float searchDuration = 1.6f;

        [Tooltip("플레이어가 이 거리 안에 있어야 수색을 유지할 수 있습니다.")]
        [SerializeField, Min(0f)] private float interactRadius = 1.2f;

        [Tooltip("아이템을 모두 가져가면 컨테이너 오브젝트를 제거할지 정합니다.")]
        [SerializeField] private bool removeWhenEmpty = true;

        [Header("전리품")]
        [Tooltip("이 컨테이너 안에 들어 있는 아이템 목록입니다. 런타임에 ItemDropper가 채웁니다.")]
        [SerializeField] private List<ItemData> lootItems = new();

        [Header("시각 표시")]
        [Tooltip("전리품 주머니나 상자를 보여줄 SpriteRenderer입니다. 비어 있으면 자식에서 자동으로 찾습니다.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        public float InteractionDuration => searchDuration;
        public string InteractionPrompt => "E 수색";
        public string ProgressText => "수색 중...";
        public IReadOnlyList<ItemData> LootItems => lootItems;
        public bool IsEmpty => lootItems.Count == 0;
        public bool RemoveWhenEmpty => removeWhenEmpty;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;
            ApplyVisual();
        }

        public void SetLoot(IEnumerable<ItemData> items)
        {
            lootItems.Clear();
            if (items != null)
            {
                foreach (ItemData item in items)
                {
                    if (item != null)
                        lootItems.Add(item);
                }
            }

            ApplyVisual();
        }

        public bool RemoveLoot(ItemData item)
        {
            if (item == null) return false;
            bool removed = lootItems.Remove(item);
            if (removed)
            {
                ApplyVisual();
                if (lootItems.Count == 0 && removeWhenEmpty)
                    Destroy(gameObject);
            }
            return removed;
        }

        public bool CanInteract(GameObject interactor)
        {
            return lootItems.Count > 0 && IsInRange(interactor);
        }

        public void Interact(GameObject interactor)
        {
            CompleteInteraction(interactor);
        }

        public bool CanContinueInteraction(GameObject interactor)
        {
            return gameObject != null && lootItems.Count > 0 && IsInRange(interactor);
        }

        public void CompleteInteraction(GameObject interactor)
        {
            LootPanelUI panel = FindFirstObjectByType<LootPanelUI>();
            if (panel == null)
            {
                Debug.LogError("LootContainer: LootPanelUI를 찾을 수 없어 전리품 창을 열 수 없습니다.");
                return;
            }

            panel.Open(this);
        }

        public void CancelInteraction(GameObject interactor)
        {
        }

        private bool IsInRange(GameObject interactor)
        {
            return interactor != null
                && Vector2.Distance(transform.position, interactor.transform.position) <= interactRadius;
        }

        private void ApplyVisual()
        {
            if (spriteRenderer == null) return;

            ItemData firstItem = lootItems.Count > 0 ? lootItems[0] : null;
            if (firstItem != null)
                spriteRenderer.sprite = firstItem.GetWorldSprite();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
