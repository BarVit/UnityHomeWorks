using UnityEngine;

public readonly struct ModuleSpec
{
    public ModuleSpec(ModuleKind kind, Sprite icon, int capacity, float cooldown, int level, int repair,
        SentrySpec sentry = default)
    {
        Sentry = sentry;
        Repair = repair;
        Level = level;
        Kind = kind;
        Icon = icon;
        Capacity = capacity;
        Cooldown = cooldown;
    }

    public ModuleKind Kind { get; }

    public Sprite Icon { get; }

    public int Capacity { get; }

    public float Cooldown { get; }

    public int Level { get; }

    public int Repair { get; }

    public SentrySpec Sentry { get; }
}
