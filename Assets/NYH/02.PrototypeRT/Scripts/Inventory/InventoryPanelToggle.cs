using UnityEngine;

namespace PrototypeRT
{
    public class InventoryPanelToggle : MonoBehaviour
    {
        [Header("인벤토리 패널")]
        [Tooltip("열고 닫을 인벤토리 UI 패널입니다. ItemGrid가 들어 있는 패널 오브젝트를 연결합니다.")]
        [SerializeField] private GameObject inventoryPanel;

        [Header("입력")]
        [Tooltip("인벤토리를 열고 닫을 키입니다.")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

        [Tooltip("게임 시작 시 인벤토리 패널을 열어둘지 정합니다. 꺼두면 시작할 때 숨겨집니다.")]
        [SerializeField] private bool openOnStart = true;

        public bool IsOpen => inventoryPanel != null && inventoryPanel.activeSelf;

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

        public void SetOpen(bool value)
        {
            if (inventoryPanel != null)
                inventoryPanel.SetActive(value);
        }
    }
}
