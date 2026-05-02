using UnityEngine;

namespace PrototypeRT
{
    public class DungeonExit : MonoBehaviour, IInteractable
    {
        [SerializeField, Min(0f)] private float interactRadius = 1.3f;

        public bool CanInteract(GameObject interactor)
        {
            return Vector2.Distance(transform.position, interactor.transform.position) <= interactRadius;
        }

        public void Interact(GameObject interactor)
        {
            // 지금은 결과창/판매를 만들지 않고, 탈출 성공 이벤트만 열어둔다.
            PrototypeRTEvents.RaisePlayerEscaped();
            Debug.Log("PrototypeRT: 탈출 성공. 판매/골드 정산은 다음 단계에서 연결합니다.");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
