using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PrototypeRT
{
    public class SellManager : MonoBehaviour
    {
        [SerializeField] private RunInventoryTracker runInventoryTracker;
        [SerializeField] private InventoryController inventoryController;
        [SerializeField] private GoldManager goldManager;
        [SerializeField] private KeyCode debugSellKey = KeyCode.None;

        public int PendingSellCount => runInventoryTracker != null ? runInventoryTracker.PendingSellCount : 0;
        public int PendingSellValue => runInventoryTracker != null ? runInventoryTracker.PendingSellValue : 0;
        public bool IsEscaped => runInventoryTracker != null && runInventoryTracker.IsEscaped;
        public bool CanSell => runInventoryTracker != null && runInventoryTracker.IsEscaped && PendingSellCount > 0;

        private void Awake()
        {
            if (runInventoryTracker == null)
                runInventoryTracker = FindFirstObjectByType<RunInventoryTracker>();
            if (inventoryController == null)
                inventoryController = FindFirstObjectByType<InventoryController>();
            if (goldManager == null)
                goldManager = GoldManager.Instance != null ? GoldManager.Instance : FindFirstObjectByType<GoldManager>();
        }

        private void Update()
        {
            if (debugSellKey != KeyCode.None && Input.GetKeyDown(debugSellKey))
                SellPendingItems();
        }

        public void SellPendingItems()
        {
            if (!ValidateReferences()) return;

            if (runInventoryTracker.PendingSellCount <= 0)
            {
                Debug.Log("SellManager: 판매할 아이템이 없습니다.");
                return;
            }

            List<InventoryItem> itemsToSell = new(runInventoryTracker.GetPendingSellItems());
            int totalGold = 0;
            foreach (InventoryItem item in itemsToSell)
            {
                if (item == null || item.ItemData == null) continue;

                int sellPrice = item.ItemData.SellPrice;
                if (inventoryController.TryRemoveItem(item))
                    totalGold += sellPrice;
            }

            // 실제로 제거된 아이템 가격만 지급해야 인벤토리 오류가 골드 복제로 이어지지 않는다.
            if (totalGold > 0)
            {
                goldManager.AddGold(totalGold);
                AudioManager.Instance?.PlaySfxByKey("Sell");
            }

            runInventoryTracker.ClearSoldItems();
        }

        public void ReEnterDungeon()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private bool ValidateReferences()
        {
            if (runInventoryTracker == null)
            {
                Debug.LogError("SellManager: RunInventoryTracker가 연결되지 않았습니다.");
                return false;
            }

            if (inventoryController == null)
            {
                Debug.LogError("SellManager: InventoryController가 연결되지 않았습니다.");
                return false;
            }

            if (goldManager == null)
            {
                goldManager = GoldManager.Instance != null ? GoldManager.Instance : FindFirstObjectByType<GoldManager>();
                if (goldManager == null)
                {
                    Debug.LogError("SellManager: GoldManager가 씬에 없습니다.");
                    return false;
                }
            }

            return true;
        }
    }
}
