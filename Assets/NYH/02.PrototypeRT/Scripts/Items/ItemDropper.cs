using System.Collections.Generic;
using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(Health))]
    public class ItemDropper : MonoBehaviour
    {
        [Header("루트 컨테이너")]
        [Tooltip("적이 죽었을 때 생성할 주머니/상자 프리팹입니다. 비어 있으면 런타임에 간단한 컨테이너를 만듭니다.")]
        [SerializeField] private LootContainer lootContainerPrefab;

        [Tooltip("기존 즉시 줍기 프리팹입니다. 새 루트 컨테이너의 레이어와 기본 시각 정보를 가져오는 fallback으로 사용합니다.")]
        [SerializeField] private WorldItemPickup worldItemPrefab;

        [Header("드랍 아이템 후보")]
        [Tooltip("이 적이 죽었을 때 컨테이너 안에 넣을 수 있는 아이템 목록입니다.")]
        [SerializeField] private ItemData[] possibleDrops;

        [Tooltip("컨테이너가 생성될 확률입니다. 1이면 항상 생성되고, 0이면 생성되지 않습니다.")]
        [SerializeField, Range(0f, 1f)] private float dropChance = 1f;

        [Tooltip("한 번 죽었을 때 컨테이너 안에 넣을 최대 아이템 수입니다.")]
        [SerializeField, Min(1)] private int maxDropCount = 1;

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
            if (possibleDrops == null || possibleDrops.Length == 0) return;
            if (Random.value > dropChance) return;

            List<ItemData> drops = RollDrops();
            if (drops.Count == 0) return;

            LootContainer container = CreateContainer();
            if (container == null) return;

            container.SetLoot(drops);
        }

        private List<ItemData> RollDrops()
        {
            List<ItemData> drops = new();
            int count = Mathf.Clamp(maxDropCount, 1, possibleDrops.Length);

            for (int i = 0; i < count; i++)
            {
                ItemData itemData = possibleDrops[Random.Range(0, possibleDrops.Length)];
                if (itemData != null)
                    drops.Add(itemData);
            }

            return drops;
        }

        private LootContainer CreateContainer()
        {
            if (lootContainerPrefab != null)
                return Instantiate(lootContainerPrefab, transform.position, Quaternion.identity);

            GameObject go = new("LootContainer");
            go.transform.position = transform.position;
            go.layer = worldItemPrefab != null ? worldItemPrefab.gameObject.layer : 6;

            SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 90;

            CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.35f;

            return go.AddComponent<LootContainer>();
        }
    }
}
