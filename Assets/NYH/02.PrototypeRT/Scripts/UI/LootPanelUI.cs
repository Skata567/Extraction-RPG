using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PrototypeRT
{
    public class LootPanelUI : MonoBehaviour
    {
        [Header("공통 인벤토리 창")]
        [Tooltip("루팅, 상자, 상점, 창고가 나중에 공유할 최상위 창입니다.")]
        [SerializeField] private GameObject panelRoot;

        [Tooltip("현재 열린 대상의 이름을 보여주는 제목 텍스트입니다.")]
        [SerializeField] private Text titleText;

        [Tooltip("아이템 이동 실패나 조작 안내를 보여주는 상태 텍스트입니다.")]
        [SerializeField] private Text hintText;

        [Header("격자 연결")]
        [Tooltip("몬스터 전리품이나 상자 내용물을 보여주는 대상 격자입니다.")]
        [SerializeField] private ItemGrid lootGrid;

        [Tooltip("플레이어 가방 격자입니다. 비어 있으면 씬에서 기존 ItemGrid를 찾아 연결합니다.")]
        [SerializeField] private ItemGrid playerBagGrid;

        [Tooltip("기존 플레이어 가방 격자를 루팅창 안에 임시로 넣어둘 위치입니다.")]
        [SerializeField] private RectTransform playerBagMount;

        [Tooltip("플레이어 인벤토리 데이터와 아이템 UI 생성을 담당하는 컨트롤러입니다.")]
        [SerializeField] private InventoryController playerInventory;

        [Header("툴팁")]
        [Tooltip("마우스를 올린 아이템의 이름, 크기, 가격을 보여주는 패널입니다.")]
        [SerializeField] private GameObject tooltipRoot;

        [Tooltip("툴팁 안에 표시되는 아이템 상세 텍스트입니다.")]
        [SerializeField] private Text tooltipText;

        private readonly List<InventoryItem> _lootViews = new();
        private LootContainer _currentContainer;
        private InventoryPanelToggle _inventoryPanelToggle;
        private bool _wasInventoryOpenBeforeLoot;
        private Transform _originalPlayerGridParent;
        private Vector2 _originalPlayerGridAnchoredPosition;
        private Vector2 _originalPlayerGridAnchorMin;
        private Vector2 _originalPlayerGridAnchorMax;
        private Vector2 _originalPlayerGridPivot;

        public void Configure(
            GameObject root,
            Text title,
            Text hint,
            ItemGrid targetGrid,
            ItemGrid bagGrid,
            RectTransform bagMount,
            InventoryController inventory,
            GameObject tooltip,
            Text tooltipLabel)
        {
            panelRoot = root;
            titleText = title;
            hintText = hint;
            lootGrid = targetGrid;
            playerBagGrid = bagGrid;
            playerBagMount = bagMount;
            playerInventory = inventory;
            tooltipRoot = tooltip;
            tooltipText = tooltipLabel;
            Hide();
            HideTooltip();
        }

        private void Awake()
        {
            if (panelRoot == null)
                panelRoot = gameObject;
            if (playerInventory == null)
                playerInventory = FindFirstObjectByType<InventoryController>();
            if (_inventoryPanelToggle == null)
                _inventoryPanelToggle = FindFirstObjectByType<InventoryPanelToggle>();
        }

        private void Update()
        {
            if (panelRoot != null && panelRoot.activeSelf && Input.GetKeyDown(KeyCode.Escape))
                Close();
        }

        public void Open(LootContainer container)
        {
            _currentContainer = container;
            if (playerInventory == null)
                playerInventory = FindFirstObjectByType<InventoryController>();
            if (_inventoryPanelToggle == null)
                _inventoryPanelToggle = FindFirstObjectByType<InventoryPanelToggle>();
            if (playerBagGrid == null)
                playerBagGrid = ResolvePlayerGrid();
            if (_inventoryPanelToggle != null)
                _wasInventoryOpenBeforeLoot = _inventoryPanelToggle.IsOpen;

            if (panelRoot != null)
                panelRoot.SetActive(true);
            if (titleText != null)
                titleText.text = "전리품";
            SetHint("아이템을 클릭하거나 가방으로 드래그해서 옮깁니다.");

            if (playerInventory != null)
            {
                MovePlayerGridIntoWindow();
                if (playerBagGrid != null)
                    playerInventory.SelectedGrid = playerBagGrid;

                // 루팅창이 열린 동안 기존 인벤토리 드래그와 충돌하지 않도록 입력을 잠근다.
                playerInventory.SetPointerInputBlocked(true);
            }

            RebuildLootGrid();
        }

        public void Close()
        {
            ClearLootViews();
            HideTooltip();
            _currentContainer = null;

            if (playerInventory != null)
                playerInventory.SetPointerInputBlocked(false);
            RestorePlayerGrid();

            Hide();
        }

        private void RebuildLootGrid()
        {
            ClearLootViews();
            if (_currentContainer == null || lootGrid == null || playerInventory == null) return;

            foreach (ItemData itemData in _currentContainer.LootItems)
            {
                InventoryItem view = playerInventory.CreateItemView(itemData, lootGrid.transform, false);
                if (view == null) continue;

                Vector2Int? pos = lootGrid.FindSpaceForItem(view);
                if (pos == null)
                {
                    Destroy(view.gameObject);
                    continue;
                }

                lootGrid.PlaceItemInternal(view, pos.Value.x, pos.Value.y);
                LootGridItemInput input = view.gameObject.AddComponent<LootGridItemInput>();
                input.Configure(this, view);
                _lootViews.Add(view);
            }

            if (_lootViews.Count == 0)
                SetHint("비어 있습니다.");
        }

        private void ClearLootViews()
        {
            if (lootGrid != null)
                lootGrid.ClearAllItems(false);

            foreach (InventoryItem view in _lootViews)
            {
                if (view != null)
                    Destroy(view.gameObject);
            }
            _lootViews.Clear();
        }

        private bool TryTransfer(InventoryItem view)
        {
            if (view == null || view.ItemData == null || _currentContainer == null || playerInventory == null) return false;

            if (playerBagGrid == null)
                playerBagGrid = ResolvePlayerGrid();
            if (playerBagGrid != null)
                playerInventory.SelectedGrid = playerBagGrid;

            ItemData itemData = view.ItemData;
            if (!playerInventory.TryAddItem(itemData))
            {
                SetHint("가방 공간이 부족합니다.");
                return false;
            }

            _currentContainer.RemoveLoot(itemData);
            lootGrid.RemoveItem(view);
            _lootViews.Remove(view);
            HideTooltip();

            if (_currentContainer == null || _currentContainer.IsEmpty)
                Close();
            else
                SetHint("아이템을 클릭하거나 가방으로 드래그해서 옮깁니다.");

            return true;
        }

        private bool IsPointerOverPlayerBag(Vector2 screenPosition)
        {
            if (playerBagGrid == null) return false;
            RectTransform rect = playerBagGrid.GetComponent<RectTransform>();
            return rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition);
        }

        private void ShowTooltip(ItemData itemData)
        {
            if (tooltipRoot == null || tooltipText == null || itemData == null) return;

            tooltipRoot.SetActive(true);
            tooltipText.text =
                $"{itemData.ItemName}\n" +
                $"크기: {itemData.Width} x {itemData.Height}\n" +
                $"타입: {itemData.ItemType}\n" +
                $"판매가: {itemData.SellPrice}G";
        }

        private void HideTooltip()
        {
            if (tooltipRoot != null)
                tooltipRoot.SetActive(false);
        }

        private void SetHint(string message)
        {
            if (hintText != null)
                hintText.text = message;
        }

        private void MovePlayerGridIntoWindow()
        {
            if (playerBagGrid == null || playerBagMount == null || _originalPlayerGridParent != null) return;

            RectTransform rect = playerBagGrid.GetComponent<RectTransform>();
            if (rect == null) return;

            _originalPlayerGridParent = rect.parent;
            _originalPlayerGridAnchoredPosition = rect.anchoredPosition;
            _originalPlayerGridAnchorMin = rect.anchorMin;
            _originalPlayerGridAnchorMax = rect.anchorMax;
            _originalPlayerGridPivot = rect.pivot;

            rect.SetParent(playerBagMount, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
        }

        private void RestorePlayerGrid()
        {
            if (playerBagGrid == null || _originalPlayerGridParent == null) return;

            RectTransform rect = playerBagGrid.GetComponent<RectTransform>();
            if (rect == null) return;

            rect.SetParent(_originalPlayerGridParent, false);
            rect.anchorMin = _originalPlayerGridAnchorMin;
            rect.anchorMax = _originalPlayerGridAnchorMax;
            rect.pivot = _originalPlayerGridPivot;
            rect.anchoredPosition = _originalPlayerGridAnchoredPosition;
            _originalPlayerGridParent = null;

            if (_inventoryPanelToggle != null)
                _inventoryPanelToggle.SetOpen(_wasInventoryOpenBeforeLoot);
        }

        private void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private ItemGrid ResolvePlayerGrid()
        {
            ItemGrid[] grids = Resources.FindObjectsOfTypeAll<ItemGrid>();
            foreach (ItemGrid grid in grids)
            {
                if (grid == null || grid == lootGrid || !grid.gameObject.scene.IsValid()) continue;
                return grid;
            }

            return null;
        }

        private sealed class LootGridItemInput : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            private LootPanelUI _panel;
            private InventoryItem _item;
            private RectTransform _rectTransform;
            private Transform _originalParent;
            private Vector2 _originalPosition;
            private Canvas _canvas;

            public void Configure(LootPanelUI panel, InventoryItem item)
            {
                _panel = panel;
                _item = item;
                _rectTransform = item != null ? item.GetComponent<RectTransform>() : null;
                _canvas = GetComponentInParent<Canvas>();
            }

            public void OnPointerClick(PointerEventData eventData)
            {
                if (eventData.button != PointerEventData.InputButton.Left) return;
                _panel?.TryTransfer(_item);
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
                _panel?.ShowTooltip(_item != null ? _item.ItemData : null);
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                _panel?.HideTooltip();
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                if (_rectTransform == null) return;

                _originalParent = _rectTransform.parent;
                _originalPosition = _rectTransform.anchoredPosition;
                _rectTransform.SetParent(_canvas != null ? _canvas.transform : _originalParent, true);
                _rectTransform.SetAsLastSibling();
            }

            public void OnDrag(PointerEventData eventData)
            {
                if (_rectTransform == null) return;
                _rectTransform.position = eventData.position;
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                if (_panel != null && _panel.IsPointerOverPlayerBag(eventData.position) && _panel.TryTransfer(_item))
                {
                    return;
                }

                if (_rectTransform == null || _originalParent == null) return;
                _rectTransform.SetParent(_originalParent, false);
                _rectTransform.anchoredPosition = _originalPosition;
            }
        }
    }
}
