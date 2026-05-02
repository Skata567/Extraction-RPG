using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PrototypeRT
{
    public class PrototypeRTBootstrapper : MonoBehaviour
    {
        private const string BootstrapName = "PrototypeRT_Bootstrapper";
        private const string RuntimeUiName = "PrototypeRT_RuntimeUI";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrapper()
        {
            if (FindFirstObjectByType<PrototypeRTBootstrapper>() != null) return;

            GameObject go = new(BootstrapName);
            DontDestroyOnLoad(go);
            go.AddComponent<PrototypeRTBootstrapper>();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            EnsurePersistentManagers();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            SetupCurrentScene();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SetupCurrentScene();
        }

        private void EnsurePersistentManagers()
        {
            if (FindFirstObjectByType<SaveManager>() == null)
                gameObject.AddComponent<SaveManager>();

            if (FindFirstObjectByType<AudioManager>() == null)
            {
                if (GetComponent<AudioSource>() == null)
                    gameObject.AddComponent<AudioSource>();
                gameObject.AddComponent<AudioManager>();
            }

            if (FindFirstObjectByType<GoldManager>() == null)
                gameObject.AddComponent<GoldManager>();
        }

        private void SetupCurrentScene()
        {
            EnsurePersistentManagers();
            EnsureRunManagers();
            EnsurePlayerComponents();
            EnsureEventSystem();
            EnsureRuntimeUI();
        }

        private void EnsureRunManagers()
        {
            GameObject managerRoot = GameObject.Find("PrototypeRT_RunManagers");
            if (managerRoot == null)
                managerRoot = new GameObject("PrototypeRT_RunManagers");

            if (FindFirstObjectByType<RunInventoryTracker>() == null)
                managerRoot.AddComponent<RunInventoryTracker>();
            if (FindFirstObjectByType<SellManager>() == null)
                managerRoot.AddComponent<SellManager>();
            if (FindFirstObjectByType<RunStateResetValidator>() == null)
                managerRoot.AddComponent<RunStateResetValidator>();
        }

        private void EnsurePlayerComponents()
        {
            Health playerHealth = FindPlayerHealth();
            if (playerHealth == null) return;

            GameObject player = playerHealth.gameObject;
            if (player.GetComponent<Stamina>() == null)
                player.AddComponent<Stamina>();
            if (player.GetComponent<PlayerDash2D>() == null)
                player.AddComponent<PlayerDash2D>();
            if (player.GetComponent<PlayerPotionUser>() == null)
                player.AddComponent<PlayerPotionUser>();
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;

            GameObject eventSystem = new("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private void EnsureRuntimeUI()
        {
            if (GameObject.Find(RuntimeUiName) != null) return;

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGo = new("Canvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            GameObject root = new(RuntimeUiName);
            RectTransform rootRect = root.AddComponent<RectTransform>();
            root.transform.SetParent(canvas.transform, false);
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            CreateHud(root.transform);
            CreateResultAndShopPanels(root.transform);
            CreateInteractionPanels(root.transform);
        }

        private void CreateHud(Transform root)
        {
            Text goldText = CreateText(root, "GoldText", "Gold: 0", new Vector2(12f, -12f), new Vector2(220f, 28f), TextAnchor.MiddleLeft, 16);
            goldText.gameObject.AddComponent<GoldTextUI>().Configure(goldText);

            Slider staminaSlider = CreateSlider(root, "StaminaSlider", new Vector2(12f, -46f), new Vector2(180f, 18f));
            StaminaBarUI staminaBar = staminaSlider.gameObject.AddComponent<StaminaBarUI>();
            staminaBar.Configure(staminaSlider, staminaSlider.fillRect != null ? staminaSlider.fillRect.GetComponent<Image>() : null);

            Text potionText = CreateText(root, "PotionText", "Potion: 0/0", new Vector2(12f, -72f), new Vector2(220f, 24f), TextAnchor.MiddleLeft, 16);
            potionText.gameObject.AddComponent<PotionCountUI>().Configure(potionText);

            Text promptText = CreateText(root, "InteractionPromptText", "E 상호작용", new Vector2(0f, 80f), new Vector2(260f, 32f), TextAnchor.MiddleCenter, 16);
            promptText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            promptText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            promptText.gameObject.AddComponent<InteractionPromptUI>().Configure(FindFirstObjectByType<PlayerInteractor>(), promptText);
            promptText.gameObject.SetActive(false);
        }

        private void CreateResultAndShopPanels(Transform root)
        {
            RunInventoryTracker tracker = FindFirstObjectByType<RunInventoryTracker>();
            SellManager sellManager = FindFirstObjectByType<SellManager>();

            GameObject resultPanel = CreatePanel(root, "RunResultPanel", new Vector2(0f, 120f), new Vector2(360f, 130f), new Color(0f, 0f, 0f, 0.78f));
            Text resultTitle = CreateText(resultPanel.transform, "Title", "", new Vector2(0f, -16f), new Vector2(320f, 32f), TextAnchor.MiddleCenter, 16);
            Text resultDetail = CreateText(resultPanel.transform, "Detail", "", new Vector2(0f, -62f), new Vector2(320f, 64f), TextAnchor.MiddleCenter, 16);
            resultPanel.SetActive(false);
            RunResultPanelUI resultUi = root.gameObject.AddComponent<RunResultPanelUI>();
            resultUi.Configure(resultPanel, resultTitle, resultDetail, tracker);

            GameObject shopPanel = CreatePanel(root, "ShopSellPanel", new Vector2(0f, -70f), new Vector2(380f, 150f), new Color(0f, 0f, 0f, 0.78f));
            Text summary = CreateText(shopPanel.transform, "Summary", "Items: 0 / Value: 0G", new Vector2(0f, -18f), new Vector2(340f, 32f), TextAnchor.MiddleCenter, 16);
            Button sellButton = CreateButton(shopPanel.transform, "SellButton", "판매", new Vector2(-82f, -72f), new Vector2(120f, 36f));
            Button reEnterButton = CreateButton(shopPanel.transform, "ReEnterButton", "재입장", new Vector2(82f, -72f), new Vector2(120f, 36f));
            shopPanel.SetActive(false);
            ShopSellPanelUI shopUi = root.gameObject.AddComponent<ShopSellPanelUI>();
            shopUi.Configure(sellManager, shopPanel, summary, sellButton, reEnterButton);
        }

        private void CreateInteractionPanels(Transform root)
        {
            GameObject progressPanel = CreatePanel(root, "InteractionProgressPanel", new Vector2(0f, -180f), new Vector2(320f, 72f), new Color(0f, 0f, 0f, 0.78f));
            Text progressText = CreateText(progressPanel.transform, "ProgressText", "", new Vector2(16f, -10f), new Vector2(288f, 24f), TextAnchor.MiddleCenter, 16);
            Slider progressSlider = CreateSlider(progressPanel.transform, "ProgressSlider", new Vector2(22f, -40f), new Vector2(276f, 18f));
            progressPanel.SetActive(false);
            InteractionProgressUI progressUi = root.gameObject.AddComponent<InteractionProgressUI>();
            progressUi.Configure(progressPanel, progressSlider, progressText);

            CreateInventoryWindow(root);
        }

        private void CreateInventoryWindow(Transform root)
        {
            GameObject window = CreatePanel(root, "LootInventoryWindow", Vector2.zero, new Vector2(940f, 560f), new Color(0.02f, 0.02f, 0.025f, 0.88f));
            RectTransform windowRect = window.GetComponent<RectTransform>();
            windowRect.anchorMin = new Vector2(0.5f, 0.5f);
            windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.pivot = new Vector2(0.5f, 0.5f);

            Text title = CreateText(window.transform, "Title", "전리품", new Vector2(0f, -14f), new Vector2(940f, 28f), TextAnchor.MiddleCenter, 18);
            title.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);

            GameObject playerSection = CreateSection(window.transform, "PlayerSection", new Vector2(-232f, -54f), new Vector2(440f, 430f), "플레이어");
            CreateEquipmentMockup(playerSection.transform);
            RectTransform bagMount = CreateMount(playerSection.transform, "PlayerBagMount", new Vector2(0f, -236f), new Vector2(320f, 320f));
            CreateSectionLabel(playerSection.transform, "BagLabel", "가방", new Vector2(-160f, -202f), new Vector2(120f, 20f));

            GameObject targetSection = CreateSection(window.transform, "TargetSection", new Vector2(232f, -54f), new Vector2(440f, 430f), "대상 컨테이너");
            ItemGrid lootGrid = CreateRuntimeItemGrid(targetSection.transform, "LootGrid", new Vector2(0f, -42f), 10, 10);

            GameObject tooltip = CreatePanel(window.transform, "TooltipPanel", new Vector2(232f, -500f), new Vector2(440f, 78f), new Color(0.03f, 0.035f, 0.04f, 0.96f));
            Text tooltipText = CreateText(tooltip.transform, "TooltipText", "", new Vector2(12f, -10f), new Vector2(416f, 58f), TextAnchor.UpperLeft, 14);
            tooltip.SetActive(false);

            Text hint = CreateText(window.transform, "Hint", "아이템을 클릭하거나 가방으로 드래그해서 옮깁니다.", new Vector2(0f, -506f), new Vector2(440f, 28f), TextAnchor.MiddleLeft, 14);
            hint.rectTransform.anchorMin = new Vector2(0f, 1f);
            hint.rectTransform.anchorMax = new Vector2(0f, 1f);

            window.SetActive(false);

            LootPanelUI lootUi = root.gameObject.AddComponent<LootPanelUI>();
            lootUi.Configure(window, title, hint, lootGrid, null, bagMount, FindFirstObjectByType<InventoryController>(), tooltip, tooltipText);
        }

        private GameObject CreateSection(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, string title)
        {
            GameObject section = CreatePanel(parent, name, anchoredPosition, size, new Color(0.08f, 0.085f, 0.095f, 0.92f));
            Text label = CreateText(section.transform, "Title", title, new Vector2(12f, -8f), new Vector2(size.x - 24f, 24f), TextAnchor.MiddleLeft, 15);
            label.color = new Color(0.86f, 0.88f, 0.9f, 1f);
            return section;
        }

        private void CreateEquipmentMockup(Transform parent)
        {
            CreateSectionLabel(parent, "EquipmentLabel", "장비", new Vector2(-160f, -42f), new Vector2(120f, 20f));
            CreateSlot(parent, "WeaponSlot", new Vector2(-82f, -82f), new Vector2(176f, 54f), "무기");
            CreateSlot(parent, "HeadSlot", new Vector2(86f, -82f), new Vector2(86f, 54f), "머리");
            CreateSlot(parent, "BodySlot", new Vector2(-58f, -146f), new Vector2(118f, 58f), "몸");
            CreateSlot(parent, "PocketSlotA", new Vector2(94f, -146f), new Vector2(54f, 58f), "주머니");
            CreateSlot(parent, "PocketSlotB", new Vector2(156f, -146f), new Vector2(54f, 58f), "주머니");
        }

        private RectTransform CreateMount(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject go = new(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return rect;
        }

        private void CreateSectionLabel(Transform parent, string name, string value, Vector2 anchoredPosition, Vector2 size)
        {
            Text label = CreateText(parent, name, value, anchoredPosition, size, TextAnchor.MiddleLeft, 13);
            label.color = new Color(0.72f, 0.75f, 0.78f, 1f);
        }

        private void CreateSlot(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, string label)
        {
            GameObject slot = CreatePanel(parent, name, anchoredPosition, size, new Color(0.11f, 0.115f, 0.125f, 0.95f));
            Image image = slot.GetComponent<Image>();
            image.raycastTarget = false;
            Text text = CreateText(slot.transform, "Label", label, Vector2.zero, size, TextAnchor.MiddleCenter, 12);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            text.rectTransform.anchoredPosition = Vector2.zero;
            text.color = new Color(0.45f, 0.48f, 0.52f, 1f);
            text.raycastTarget = false;
        }

        private ItemGrid CreateRuntimeItemGrid(Transform parent, string name, Vector2 anchoredPosition, int width, int height)
        {
            GameObject go = new(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            Image image = go.AddComponent<Image>();
            ItemGrid grid = go.AddComponent<ItemGrid>();
            go.transform.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            image.color = new Color(0.035f, 0.04f, 0.05f, 0.96f);
            grid.ConfigureSize(width, height);
            CreateGridCells(rect, width, height);
            return grid;
        }

        private void CreateGridCells(RectTransform gridRoot, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    GameObject cell = new($"Cell_{x}_{y}");
                    RectTransform rect = cell.AddComponent<RectTransform>();
                    Image image = cell.AddComponent<Image>();
                    cell.transform.SetParent(gridRoot, false);
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0f, 1f);
                    rect.anchoredPosition = new Vector2(x * ItemGrid.TileWidth, -y * ItemGrid.TileHeight);
                    rect.sizeDelta = new Vector2(ItemGrid.TileWidth - 1f, ItemGrid.TileHeight - 1f);
                    image.color = new Color(0.16f, 0.17f, 0.18f, 0.42f);
                    image.raycastTarget = false;
                }
            }
        }

        private GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject go = new(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            Image image = go.AddComponent<Image>();
            image.color = color;
            go.transform.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return go;
        }

        private Text CreateText(Transform parent, string name, string value, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment, int fontSize)
        {
            GameObject go = new(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            Text text = go.AddComponent<Text>();
            go.transform.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = value;
            return text;
        }

        private Slider CreateSlider(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject go = new(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            Slider slider = go.AddComponent<Slider>();
            go.transform.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            GameObject background = new("Background");
            RectTransform bgRect = background.AddComponent<RectTransform>();
            Image bgImage = background.AddComponent<Image>();
            background.transform.SetParent(go.transform, false);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            bgImage.color = new Color(0.12f, 0.12f, 0.12f, 0.8f);

            GameObject fill = new("Fill");
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            Image fillImage = fill.AddComponent<Image>();
            fill.transform.SetParent(go.transform, false);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillImage.color = new Color(0.25f, 0.85f, 0.45f, 0.95f);

            slider.fillRect = fillRect;
            slider.targetGraphic = fillImage;
            slider.transition = Selectable.Transition.None;
            return slider;
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject go = new(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            Image image = go.AddComponent<Image>();
            Button button = go.AddComponent<Button>();
            go.transform.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            image.color = new Color(0.18f, 0.22f, 0.28f, 0.95f);
            button.targetGraphic = image;

            Text text = CreateText(go.transform, "Text", label, Vector2.zero, size, TextAnchor.MiddleCenter, 16);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            text.rectTransform.anchoredPosition = Vector2.zero;
            text.raycastTarget = false;
            return button;
        }

        private Health FindPlayerHealth()
        {
            Health[] healths = FindObjectsByType<Health>(FindObjectsSortMode.None);
            foreach (Health health in healths)
            {
                if (health.Team == Team.Player)
                    return health;
            }
            return null;
        }
    }
}
