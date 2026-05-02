using UnityEngine;

namespace PrototypeRT
{
    public class DungeonExit : MonoBehaviour, IInteractable, IProgressInteractable
    {
        [Header("탈출 상호작용")]
        [Tooltip("플레이어가 이 거리 안에 있어야 탈출 준비를 시작하고 유지할 수 있습니다.")]
        [SerializeField, Min(0f)] private float interactRadius = 1.3f;

        [Tooltip("탈출구에서 탈출 준비가 완료되기까지 걸리는 시간입니다.")]
        [SerializeField, Min(0.1f)] private float exitDuration = 2.5f;

        public float InteractionDuration => exitDuration;
        public string InteractionPrompt => "E 탈출";
        public string ProgressText => "탈출 준비 중...";

        public bool CanInteract(GameObject interactor)
        {
            return interactor != null
                && Vector2.Distance(transform.position, interactor.transform.position) <= interactRadius;
        }

        public void Interact(GameObject interactor)
        {
            CompleteInteraction(interactor);
        }

        public bool CanContinueInteraction(GameObject interactor)
        {
            return CanInteract(interactor);
        }

        public void CompleteInteraction(GameObject interactor)
        {
            // 탈출 완료 이벤트만 발행해 정산, 판매, 결과 UI는 기존 흐름을 그대로 재사용한다.
            PrototypeRTEvents.RaisePlayerEscaped();
            Debug.Log("PrototypeRT: 탈출 성공. 판매/골드 정산은 기존 탈출 성공 흐름에서 처리합니다.");
        }

        public void CancelInteraction(GameObject interactor)
        {
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
