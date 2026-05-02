using UnityEngine;

namespace PrototypeRT
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float interactRadius = 1.2f;
        [SerializeField] private LayerMask interactLayers;

        public bool HasInteractable => CurrentInteractable != null;
        public IInteractable CurrentInteractable { get; private set; }

        private void Update()
        {
            CurrentInteractable = FindBestInteractable();
            if (PrototypeDungeonManager.IsRunEnded) return;

            if (Input.GetKeyDown(KeyCode.E))
                CurrentInteractable?.Interact(gameObject);
        }

        private void TryInteract()
        {
            CurrentInteractable = FindBestInteractable();
            CurrentInteractable?.Interact(gameObject);
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
