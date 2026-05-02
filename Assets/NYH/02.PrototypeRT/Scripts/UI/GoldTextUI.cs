using UnityEngine;
using UnityEngine.UI;

namespace PrototypeRT
{
    public class GoldTextUI : MonoBehaviour
    {
        [SerializeField] private Text goldText;
        [SerializeField] private string format = "Gold: {0}";

        public void Configure(Text text)
        {
            goldText = text;
        }

        private void Awake()
        {
            if (goldText == null)
                goldText = GetComponent<Text>();
        }

        private void OnEnable()
        {
            PrototypeRTEvents.OnGoldChanged += Refresh;
        }

        private void Start()
        {
            GoldManager goldManager = GoldManager.Instance != null ? GoldManager.Instance : FindFirstObjectByType<GoldManager>();
            Refresh(goldManager != null ? goldManager.CurrentGold : 0);
        }

        private void OnDisable()
        {
            PrototypeRTEvents.OnGoldChanged -= Refresh;
        }

        private void Refresh(int currentGold)
        {
            if (goldText == null)
            {
                Debug.LogError("GoldTextUI: 골드를 표시할 Text가 연결되지 않았습니다.");
                return;
            }

            // UI는 골드 값을 직접 계산하지 않고 매니저가 알려준 값만 표시해, 경제 로직과 화면 표시를 분리한다.
            goldText.text = string.Format(format, currentGold);
        }
    }
}
