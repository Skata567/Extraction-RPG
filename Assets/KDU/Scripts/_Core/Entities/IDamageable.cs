public interface IDamageable
{
    int CurrentHp { get; }
    int MaxHp { get; }
    bool IsDead { get; }
    void TakeDamage(int amount);
    void Heal(int amount);
}
