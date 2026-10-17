using UnityEngine;

public static class ShieldColors
{
    public const int MaxLevel = 5;

    private static readonly Color[] Colors =
    {
        new Color(0.86f, 0.88f, 0.92f),
        new Color(0.35f, 0.88f, 1f),
        new Color(0.3f, 0.5f, 1f),
        new Color(0.72f, 0.38f, 1f),
        new Color(1f, 0.32f, 0.3f)
    };

    public static Color Get(int level)
    {
        return Colors[Mathf.Clamp(level, 1, MaxLevel) - 1];
    }
}
