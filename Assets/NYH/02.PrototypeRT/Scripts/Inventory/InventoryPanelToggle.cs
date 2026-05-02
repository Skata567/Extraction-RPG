using UnityEngine;

namespace PrototypeRT
{
    public class InventoryPanelToggle : MonoBehaviour
    {
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
        [SerializeField] private bool openOnStart = true;

        private void Start()
        {
            if (inventoryPanel == null)
                Debug.LogError("InventoryPanelToggle: Inventory Panel이 연결되지 않았습니다.");
            else
                inventoryPanel.SetActive(openOnStart);
        }

        private void Update()
        {
            if (inventoryPanel == null) return;

            // 인벤토리는 전투 중 자주 열고 닫기 때문에 입력 처리를 UI 전용 컴포넌트에 둔다.
            if (Input.GetKeyDown(toggleKey))
                inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }
}
