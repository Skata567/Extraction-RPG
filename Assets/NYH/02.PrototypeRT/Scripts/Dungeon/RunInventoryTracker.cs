using System.Collections.Generic;
using UnityEngine;

namespace PrototypeRT
{
    public class RunInventoryTracker : MonoBehaviour
    {
        [SerializeField] private InventoryController inventoryController;

        private readonly List<InventoryItem> _runItems = new();
        private readonly List<InventoryItem> _pendingSellItems = new();
        private bool _isEscaped;

        public IReadOnlyList<InventoryItem> PendingSellItems => _pendingSellItems;
        public bool IsEscaped => _isEscaped;

        public int PendingSellValue
        {
            get
            {
                CleanupDestroyedItems();

                int total = 0;
                foreach (InventoryItem item in _pendingSellItems)
                {
                    if (item != null && item.ItemData != null)
                        total += item.ItemData.SellPrice;
                }
                return total;
            }
        }

        public int PendingSellCount
        {
            get
            {
                CleanupDestroyedItems();
                return _pendingSellItems.Count;
            }
        }

        private void Awake()
        {
            if (inventoryController == null)
                inventoryController = FindFirstObjectByType<InventoryController>();
        }

        private void OnEnable()
        {
            if (inventoryController == null)
                inventoryController = FindFirstObjectByType<InventoryController>();

            if (inventoryController != null)
                inventoryController.OnItemAdded += HandleItemAdded;
            else
                Debug.LogError("RunInventoryTracker: InventoryController를 찾을 수 없습니다. 원정 아이템 추적이 동작하지 않습니다.");

            PrototypeRTEvents.OnDied += HandleDied;
            PrototypeRTEvents.OnPlayerEscaped += HandlePlayerEscaped;
        }

        private void OnDisable()
        {
            if (inventoryController != null)
                inventoryController.OnItemAdded -= HandleItemAdded;

            PrototypeRTEvents.OnDied -= HandleDied;
            PrototypeRTEvents.OnPlayerEscaped -= HandlePlayerEscaped;
        }

        public IReadOnlyList<InventoryItem> GetPendingSellItems()
        {
            CleanupDestroyedItems();
            return _pendingSellItems;
        }

        public void ClearSoldItems()
        {
            _pendingSellItems.Clear();
            _runItems.Clear();
            PrototypeRTEvents.RaiseRunItemsChanged();
        }

        private void HandleItemAdded(InventoryItem item)
        {
            if (item == null || _isEscaped) return;
            if (_runItems.Contains(item)) return;

            // 던전 안에서 새로 얻은 아이템만 별도로 기록해야 사망 손실과 탈출 판매를 분리할 수 있다.
            _runItems.Add(item);
            PrototypeRTEvents.RaiseRunItemsChanged();
        }

        private void HandlePlayerEscaped()
        {
            if (_isEscaped) return;

            _isEscaped = true;
            _pendingSellItems.Clear();

            foreach (InventoryItem item in _runItems)
            {
                if (item != null && item.ItemData != null)
                    _pendingSellItems.Add(item);
            }

            PrototypeRTEvents.RaiseRunItemsChanged();
        }

        private void HandleDied(Health health)
        {
            if (health == null || health.Team != Team.Player) return;
            DiscardRunItems();
        }

        private void DiscardRunItems()
        {
            if (inventoryController == null) return;

            // 사망 시 이번 원정에서 얻은 아이템만 제거한다. 기존 확정 보유품/장착품 처리는 v0.1 범위 밖이다.
            List<InventoryItem> itemsToRemove = new(_runItems);
            foreach (InventoryItem item in itemsToRemove)
            {
                if (item != null)
                    inventoryController.TryRemoveItem(item);
            }

            _runItems.Clear();
            _pendingSellItems.Clear();
            PrototypeRTEvents.RaiseRunItemsChanged();
        }

        private void CleanupDestroyedItems()
        {
            _runItems.RemoveAll(item => item == null);
            _pendingSellItems.RemoveAll(item => item == null);
        }
    }
}
