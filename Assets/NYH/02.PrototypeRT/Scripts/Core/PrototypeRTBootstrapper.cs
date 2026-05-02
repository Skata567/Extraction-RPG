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
        }

        private void CreateHud(Transform root)
        {
            Text goldText = CreateText(root, "GoldText", "Gold: 0", new Vector2(12f, -12f), new Vector2(220f, 28f), TextAnchor.MiddleLeft);
            goldText.gameObject.AddComponent<GoldTextUI>().Configure(goldText);

            Slider staminaSlider = CreateSlider(root, "StaminaSlider", new Vector2(12f, -46f), new Vector2(180f, 18f));
            StaminaBarUI staminaBar = staminaSlider.gameObject.AddComponent<StaminaBarUI>();
            staminaBar.Configure(staminaSlider, staminaSlider.fillRect != null ? staminaSlider.fillRect.GetComponent<Image>() : null);

            Text potionText = CreateText(root, "PotionText", "Potion: 0/0", new Vector2(12f, -72f), new Vector2(220f, 24f), TextAnchor.MiddleLeft);
            potionText.gameObject.AddComponent<PotionCountUI>().Configure(potionText);

            Text promptText = CreateText(root, "InteractionPromptText", "E 상호작용", new Vector2(0f, 80f), new Vector2(260f, 32f), TextAnchor.MiddleCenter);
            promptText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            promptText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            promptText.gameObject.AddComponent<InteractionPromptUI>().Configure(FindFirstObjectByType<PlayerInteractor>(), promptText);
            promptText.gameObject.SetActive(false);
        }

        private void CreateResultAndShopPanels(Transform root)
        {
            RunInventoryTracker tracker = FindFirstObjectByType<RunInventoryTracker>();
            SellManager sellManager = FindFirstObjectByType<SellManager>();

            GameObject resultPanel = CreatePanel(root, "RunResultPanel", new Vector2(0f, 120f), new Vector2(360f, 130f));
            Text resultTitle = CreateText(resultPanel.transform, "Title", "", new Vector2(0f, -16f), new Vector2(320f, 32f), TextAnchor.MiddleCenter);
            Text resultDetail = CreateText(resultPanel.transform, "Detail", "", new Vector2(0f, -62f), new Vector2(320f, 64f), TextAnchor.MiddleCenter);
            resultPanel.SetActive(false);
            RunResultPanelUI resultUi = root.gameObject.AddComponent<RunResultPanelUI>();
            resultUi.Configure(resultPanel, resultTitle, resultDetail, tracker);

            GameObject shopPanel = CreatePanel(root, "ShopSellPanel", new Vector2(0f, -70f), new Vector2(380f, 150f));
            Text summary = CreateText(shopPanel.transform, "Summary", "Items: 0 / Value: 0G", new Vector2(0f, -18f), new Vector2(340f, 32f), TextAnchor.MiddleCenter);
            Button sellButton = CreateButton(shopPanel.transform, "SellButton", "판매", new Vector2(-82f, -72f), new Vector2(120f, 36f));
            Button reEnterButton = CreateButton(shopPanel.transform, "ReEnterButton", "재입장", new Vector2(82f, -72f), new Vector2(120f, 36f));
            shopPanel.SetActive(false);
            ShopSellPanelUI shopUi = root.gameObject.AddComponent<ShopSellPanelUI>();
            shopUi.Configure(sellManager, shopPanel, summary, sellButton, reEnterButton);
        }

        private GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject go = new(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            Image image = go.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.78f);
            go.transform.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return go;
        }

        private Text CreateText(Transform parent, string name, string value, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment)
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
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 16;
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

            Text text = CreateText(go.transform, "Text", label, Vector2.zero, size, TextAnchor.MiddleCenter);
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
