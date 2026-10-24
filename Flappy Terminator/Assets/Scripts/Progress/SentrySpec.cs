public readonly struct SentrySpec
{
    public SentrySpec(float damagePerSecond, float lifetime)
    {
        DamagePerSecond = damagePerSecond;
        Lifetime = lifetime;
    }

    public float DamagePerSecond { get; }

    public float Lifetime { get; }
}
