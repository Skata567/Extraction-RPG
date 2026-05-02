using UnityEngine;
using UnityEngine.UI;

namespace PrototypeRT
{
    public class ShopSellPanelUI : MonoBehaviour
    {
        [SerializeField] private SellManager sellManager;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text summaryText;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button reEnterButton;
        [SerializeField] private string summaryFormat = "Items: {0} / Value: {1}G";

        public void Configure(SellManager manager, GameObject root, Text summary, Button sell, Button reEnter)
        {
            if (sellButton != null)
                sellButton.onClick.RemoveListener(HandleSellClicked);
            if (reEnterButton != null)
                reEnterButton.onClick.RemoveListener(HandleReEnterClicked);

            sellManager = manager;
            panelRoot = root;
            summaryText = summary;
            sellButton = sell;
            reEnterButton = reEnter;

            if (sellButton != null)
                sellButton.onClick.AddListener(HandleSellClicked);
            if (reEnterButton != null)
                reEnterButton.onClick.AddListener(HandleReEnterClicked);

            Refresh();
        }

        private void Awake()
        {
            if (sellManager == null)
                sellManager = FindFirstObjectByType<SellManager>();
            if (panelRoot == null)
                panelRoot = gameObject;
        }

        private void OnEnable()
        {
            PrototypeRTEvents.OnPlayerEscaped += HandlePlayerEscaped;
            PrototypeRTEvents.OnRunItemsChanged += Refresh;

            if (sellButton != null)
                sellButton.onClick.AddListener(HandleSellClicked);
            if (reEnterButton != null)
                reEnterButton.onClick.AddListener(HandleReEnterClicked);
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            PrototypeRTEvents.OnPlayerEscaped -= HandlePlayerEscaped;
            PrototypeRTEvents.OnRunItemsChanged -= Refresh;

            if (sellButton != null)
                sellButton.onClick.RemoveListener(HandleSellClicked);
            if (reEnterButton != null)
                reEnterButton.onClick.RemoveListener(HandleReEnterClicked);
        }

        private void HandlePlayerEscaped()
        {
            Refresh();
        }

        private void HandleSellClicked()
        {
            if (sellManager == null)
            {
                Debug.LogError("ShopSellPanelUI: SellManager가 연결되지 않았습니다.");
                return;
            }

            sellManager.SellPendingItems();
            Refresh();
        }

        private void HandleReEnterClicked()
        {
            if (sellManager == null)
            {
                Debug.LogError("ShopSellPanelUI: SellManager가 연결되지 않았습니다.");
                return;
            }

            sellManager.ReEnterDungeon();
        }

        private void Refresh()
        {
            bool isEscaped = sellManager != null && sellManager.IsEscaped;
            if (panelRoot != null)
                panelRoot.SetActive(isEscaped);

            if (!isEscaped) return;

            int count = sellManager.PendingSellCount;
            int value = sellManager.PendingSellValue;

            if (summaryText != null)
                summaryText.text = string.Format(summaryFormat, count, value);

            // 탈출 후에만 판매/재입장 버튼을 열어, 던전 중 실수로 정산되는 상황을 막는다.
            if (sellButton != null)
                sellButton.interactable = sellManager.CanSell;
            if (reEnterButton != null)
                reEnterButton.interactable = !sellManager.CanSell;
        }
    }
}
