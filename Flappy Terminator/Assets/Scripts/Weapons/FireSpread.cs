using UnityEngine;

public static class FireSpread
{
    private const float ConeStep = 12f;
    private const float NarrowConeStep = 4f;
    private const float SideAngle = 60f;

    public static float GetAngle(FirePattern pattern, int barrel, int barrels)
    {
        switch (pattern)
        {
            case FirePattern.Cone:
                return Spread(barrel, barrels, ConeStep);

            case FirePattern.NarrowCone:
                return Spread(barrel, barrels, NarrowConeStep);

            case FirePattern.ForwardAndSides:
                return barrel < 2 ? 0f : (barrel % 2 == 0 ? SideAngle : -SideAngle);

            default:
                return 0f;
        }
    }

    private static float Spread(int barrel, int barrels, float step)
    {
        if (barrels <= 1)
            return 0f;

        return (barrel - (barrels - 1) * 0.5f) * step;
    }
}
