using UnityEngine;

namespace PrototypeRT
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("상호작용 범위")]
        [Tooltip("플레이어가 아이템이나 출구와 상호작용할 수 있는 거리입니다.")]
        [SerializeField, Min(0f)] private float interactRadius = 1.2f;

        [Tooltip("E 키로 상호작용할 오브젝트 레이어입니다. 아이템과 출구는 Interactable 레이어에 있어야 합니다.")]
        [SerializeField] private LayerMask interactLayers;

        public bool HasInteractable => CurrentInteractable != null;
        public IInteractable CurrentInteractable { get; private set; }
        public bool IsInteractionInProgress => _progressTarget != null;

        private IProgressInteractable _progressTarget;
        private GameObject _progressTargetObject;
        private float _progressElapsed;

        private void OnEnable()
        {
            PrototypeRTEvents.OnDamaged += HandleDamaged;
        }

        private void OnDisable()
        {
            PrototypeRTEvents.OnDamaged -= HandleDamaged;
            CancelProgress();
        }

        private void Update()
        {
            if (UpdateProgressInteraction()) return;

            CurrentInteractable = FindBestInteractable();
            if (PrototypeDungeonManager.IsRunEnded) return;

            if (Input.GetKeyDown(KeyCode.E))
                TryInteract();
        }

        private void TryInteract()
        {
            CurrentInteractable = FindBestInteractable();
            if (CurrentInteractable == null) return;

            if (CurrentInteractable is IProgressInteractable progressInteractable)
                StartProgress(progressInteractable);
            else
                CurrentInteractable.Interact(gameObject);
        }

        private void StartProgress(IProgressInteractable progressInteractable)
        {
            if (progressInteractable == null || !progressInteractable.CanContinueInteraction(gameObject)) return;

            _progressTarget = progressInteractable;
            _progressTargetObject = (progressInteractable as Component)?.gameObject;
            _progressElapsed = 0f;
            PrototypeRTEvents.RaiseInteractionProgressStarted(progressInteractable.ProgressText, progressInteractable.InteractionDuration);
            PrototypeRTEvents.RaiseInteractionProgressUpdated(0f);
        }

        private bool UpdateProgressInteraction()
        {
            if (_progressTarget == null) return false;

            if (PrototypeDungeonManager.IsRunEnded
                || _progressTargetObject == null
                || !_progressTarget.CanContinueInteraction(gameObject))
            {
                CancelProgress();
                return true;
            }

            float duration = Mathf.Max(0.01f, _progressTarget.InteractionDuration);
            _progressElapsed += Time.deltaTime;
            PrototypeRTEvents.RaiseInteractionProgressUpdated(_progressElapsed / duration);

            if (_progressElapsed < duration) return true;

            IProgressInteractable completedTarget = _progressTarget;
            _progressTarget = null;
            _progressTargetObject = null;
            _progressElapsed = 0f;
            PrototypeRTEvents.RaiseInteractionProgressUpdated(1f);
            PrototypeRTEvents.RaiseInteractionProgressCompleted();
            completedTarget.CompleteInteraction(gameObject);
            return true;
        }

        private void CancelProgress()
        {
            if (_progressTarget == null) return;

            _progressTarget.CancelInteraction(gameObject);
            _progressTarget = null;
            _progressTargetObject = null;
            _progressElapsed = 0f;
            PrototypeRTEvents.RaiseInteractionProgressCanceled();
        }

        private void HandleDamaged(Health damagedHealth, DamageInfo damageInfo)
        {
            if (damagedHealth == null || damagedHealth.Team != Team.Player) return;
            CancelProgress();
        }

        private IInteractable FindBestInteractable()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayers);
            float bestDistance = float.MaxValue;
            IInteractable best = null;

            foreach (Collider2D hit in hits)
            {
                IInteractable interactable = hit.GetComponentInParent<IInteractable>();
                if (interactable == null || !interactable.CanInteract(gameObject)) continue;

                float distance = (hit.transform.position - transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = interactable;
                }
            }

            return best;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
