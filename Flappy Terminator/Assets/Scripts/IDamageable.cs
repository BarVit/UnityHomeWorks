public interface IDamageable
{
    bool IsVulnerable { get; }

    void TakeDamage(int damage);
}
