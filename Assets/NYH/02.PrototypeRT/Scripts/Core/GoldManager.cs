using UnityEngine;

namespace PrototypeRT
{
    public class GoldManager : MonoBehaviour
    {
        private static GoldManager _instance;

        [SerializeField, Min(0)] private int startingGold;
        [SerializeField] private SaveManager saveManager;

        public static GoldManager Instance => _instance;
        public int CurrentGold { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (saveManager == null)
                saveManager = SaveManager.Instance != null ? SaveManager.Instance : FindFirstObjectByType<SaveManager>();

            CurrentGold = saveManager != null
                ? saveManager.LoadGold(startingGold)
                : Mathf.Max(0, startingGold);
        }

        private void Start()
        {
            PrototypeRTEvents.RaiseGoldChanged(CurrentGold);
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            // 골드는 원정이 끝난 뒤에도 남아야 하므로, v0.1에서는 이 매니저만 씬 재로드를 넘어 유지한다.
            CurrentGold += amount;
            saveManager?.SaveGold(CurrentGold);
            PrototypeRTEvents.RaiseGoldChanged(CurrentGold);
            AudioManager.Instance?.PlaySfxByKey("Gold");
        }
    }
}
