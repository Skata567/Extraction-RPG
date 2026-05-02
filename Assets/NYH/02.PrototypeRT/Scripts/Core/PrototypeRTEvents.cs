using System;
using UnityEngine;

namespace PrototypeRT
{
    public static class PrototypeRTEvents
    {
        public static event Action<Health> OnDied;
        public static event Action<Health, DamageInfo> OnDamaged;
        public static event Action<Health, int> OnHealed;
        public static event Action<ItemData> OnItemPickedUp;
        public static event Action<ItemData> OnWeaponEquipped;
        public static event Action OnInventoryChanged;
        public static event Action OnPlayerEscaped;
        public static event Action<int> OnGoldChanged;
        public static event Action OnRunItemsChanged;
        public static event Action<float, float> OnStaminaChanged;
        public static event Action<int, int> OnPotionCountChanged;
        public static event Action<string, float> OnInteractionProgressStarted;
        public static event Action<float> OnInteractionProgressUpdated;
        public static event Action OnInteractionProgressCanceled;
        public static event Action OnInteractionProgressCompleted;

        // 정적 이벤트는 씬 재시작 시 구독자가 남기 쉬워서, 프로토타입 매니저가 명시적으로 초기화할 수 있게 둔다.
        public static void ClearAll()
        {
            OnDied = null;
            OnDamaged = null;
            OnHealed = null;
            OnItemPickedUp = null;
            OnWeaponEquipped = null;
            OnInventoryChanged = null;
            OnPlayerEscaped = null;
            OnGoldChanged = null;
            OnRunItemsChanged = null;
            OnStaminaChanged = null;
            OnPotionCountChanged = null;
            OnInteractionProgressStarted = null;
            OnInteractionProgressUpdated = null;
            OnInteractionProgressCanceled = null;
            OnInteractionProgressCompleted = null;
        }

        public static void RaiseDied(Health health) => OnDied?.Invoke(health);
        public static void RaiseDamaged(Health health, DamageInfo damageInfo) => OnDamaged?.Invoke(health, damageInfo);
        public static void RaiseHealed(Health health, int amount) => OnHealed?.Invoke(health, amount);
        public static void RaiseItemPickedUp(ItemData itemData) => OnItemPickedUp?.Invoke(itemData);
        public static void RaiseWeaponEquipped(ItemData itemData) => OnWeaponEquipped?.Invoke(itemData);
        public static void RaiseInventoryChanged() => OnInventoryChanged?.Invoke();
        public static void RaisePlayerEscaped() => OnPlayerEscaped?.Invoke();
        public static void RaiseGoldChanged(int currentGold) => OnGoldChanged?.Invoke(currentGold);
        public static void RaiseRunItemsChanged() => OnRunItemsChanged?.Invoke();
        public static void RaiseStaminaChanged(float current, float max) => OnStaminaChanged?.Invoke(current, max);
        public static void RaisePotionCountChanged(int current, int max) => OnPotionCountChanged?.Invoke(current, max);
        public static void RaiseInteractionProgressStarted(string label, float duration) => OnInteractionProgressStarted?.Invoke(label, duration);
        public static void RaiseInteractionProgressUpdated(float progress) => OnInteractionProgressUpdated?.Invoke(progress);
        public static void RaiseInteractionProgressCanceled() => OnInteractionProgressCanceled?.Invoke();
        public static void RaiseInteractionProgressCompleted() => OnInteractionProgressCompleted?.Invoke();
    }
}
