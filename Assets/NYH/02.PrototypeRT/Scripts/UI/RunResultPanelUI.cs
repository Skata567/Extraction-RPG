using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeRT
{
    public class RunResultPanelUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text detailText;
        [SerializeField] private RunInventoryTracker runInventoryTracker;

        public void Configure(GameObject root, Text title, Text detail, RunInventoryTracker tracker)
        {
            panelRoot = root;
            titleText = title;
            detailText = detail;
            runInventoryTracker = tracker;
        }

        private void Awake()
        {
            if (panelRoot == null)
                panelRoot = gameObject;
            if (runInventoryTracker == null)
                runInventoryTracker = FindFirstObjectByType<RunInventoryTracker>();
        }

        private void OnEnable()
        {
            PrototypeRTEvents.OnPlayerEscaped += HandleEscaped;
            PrototypeRTEvents.OnDied += HandleDied;
        }

        private void Start()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void OnDisable()
        {
            PrototypeRTEvents.OnPlayerEscaped -= HandleEscaped;
            PrototypeRTEvents.OnDied -= HandleDied;
        }

        private void HandleEscaped()
        {
            StartCoroutine(ShowEscapedNextFrame());
            AudioManager.Instance?.PlaySfxByKey("Escape");
        }

        private IEnumerator ShowEscapedNextFrame()
        {
            // 탈출 이벤트를 여러 컴포넌트가 동시에 받기 때문에, 원정 아이템 정산이 끝난 다음 프레임에 결과를 읽는다.
            yield return null;
            Show("탈출 성공", BuildEscapeDetail());
        }

        private void HandleDied(Health health)
        {
            if (health == null || health.Team != Team.Player) return;

            Show("사망", "이번 원정에서 획득한 아이템을 잃었습니다.");
            AudioManager.Instance?.PlaySfxByKey("Death");
        }

        private string BuildEscapeDetail()
        {
            int count = runInventoryTracker != null ? runInventoryTracker.PendingSellCount : 0;
            int value = runInventoryTracker != null ? runInventoryTracker.PendingSellValue : 0;
            return $"판매 가능 아이템: {count}\n예상 판매 골드: {value}";
        }

        private void Show(string title, string detail)
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);
            if (titleText != null)
                titleText.text = title;
            if (detailText != null)
                detailText.text = detail;
        }
    }
}
