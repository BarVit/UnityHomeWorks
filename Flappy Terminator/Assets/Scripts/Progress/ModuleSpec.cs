using UnityEngine;

public readonly struct ModuleSpec
{
    public ModuleSpec(ModuleKind kind, Sprite icon, int capacity, float cooldown)
    {
        Kind = kind;
        Icon = icon;
        Capacity = capacity;
        Cooldown = cooldown;
    }

    public ModuleKind Kind { get; }

    public Sprite Icon { get; }

    public int Capacity { get; }

    public float Cooldown { get; }
}
