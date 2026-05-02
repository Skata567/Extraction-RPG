using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(Health))]
    public class ItemDropper : MonoBehaviour
    {
        [SerializeField] private WorldItemPickup worldItemPrefab;
        [SerializeField] private ItemData[] possibleDrops;
        [SerializeField, Range(0f, 1f)] private float dropChance = 1f;

        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            PrototypeRTEvents.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            PrototypeRTEvents.OnDied -= HandleDied;
        }

        private void HandleDied(Health deadHealth)
        {
            if (deadHealth != _health) return;
            if (worldItemPrefab == null || possibleDrops == null || possibleDrops.Length == 0) return;
            if (Random.value > dropChance) return;

            ItemData itemData = possibleDrops[Random.Range(0, possibleDrops.Length)];
            WorldItemPickup pickup = Instantiate(worldItemPrefab, transform.position, Quaternion.identity);
            pickup.SetItem(itemData);
        }
    }
}
