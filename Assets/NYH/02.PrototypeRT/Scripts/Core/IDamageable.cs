namespace PrototypeRT
{
    public interface IDamageable
    {
        int CurrentHp { get; }
        int MaxHp { get; }
        bool IsDead { get; }
        Team Team { get; }
        void TakeDamage(DamageInfo damageInfo);
        void Heal(int amount);
    }
}
