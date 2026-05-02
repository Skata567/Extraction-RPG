using UnityEngine;

namespace PrototypeRT
{
    public class RunStateResetValidator : MonoBehaviour
    {
        [SerializeField] private bool logOnStart = true;

        private void Start()
        {
            if (!logOnStart) return;

            Health playerHealth = FindPlayerHealth();
            Stamina stamina = playerHealth != null ? playerHealth.GetComponent<Stamina>() : null;
            PlayerPotionUser potionUser = playerHealth != null ? playerHealth.GetComponent<PlayerPotionUser>() : null;
            RunInventoryTracker tracker = FindFirstObjectByType<RunInventoryTracker>();

            // 재입장 버그는 조용히 숨어 있다가 나중에 크게 터지므로, v0.1에서는 시작 로그로 초기화 상태를 확인한다.
            Debug.Log(
                $"RunStateResetValidator: HP={FormatHp(playerHealth)}, " +
                $"Stamina={FormatStamina(stamina)}, " +
                $"Potions={FormatPotions(potionUser)}, " +
                $"PendingSell={FormatPendingSell(tracker)}");
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

        private string FormatHp(Health health)
        {
            return health != null ? $"{health.CurrentHp}/{health.MaxHp}" : "None";
        }

        private string FormatStamina(Stamina stamina)
        {
            return stamina != null ? $"{stamina.CurrentStamina:0}/{stamina.MaxStamina:0}" : "None";
        }

        private string FormatPotions(PlayerPotionUser potionUser)
        {
            return potionUser != null ? $"{potionUser.CurrentPotionCount}/{potionUser.MaxPotionCount}" : "None";
        }

        private string FormatPendingSell(RunInventoryTracker tracker)
        {
            return tracker != null ? tracker.PendingSellCount.ToString() : "None";
        }
    }
}
