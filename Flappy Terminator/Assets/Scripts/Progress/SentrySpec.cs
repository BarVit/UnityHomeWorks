public readonly struct SentrySpec
{
    public SentrySpec(float damagePerSecond, float attackTime, float lifetime, float reload)
    {
        DamagePerSecond = damagePerSecond;
        AttackTime = attackTime;
        Lifetime = lifetime;
        Reload = reload;
    }

    public float DamagePerSecond { get; }

    public float AttackTime { get; }

    public float Lifetime { get; }

    public float Reload { get; }
}
