using UnityEngine;

public static class FireSpread
{
    private const float ConeStep = 12f;
    private const float NarrowConeStep = 4f;
    private const float SideAngle = 60f;

    public static float GetAngle(FirePattern pattern, int barrel, int rank, int barrels)
    {
        switch (pattern)
        {
            case FirePattern.Cone:
                return Spread(rank, barrels, ConeStep);

            case FirePattern.NarrowCone:
                return Spread(rank, barrels, NarrowConeStep);

            case FirePattern.ForwardAndSides:
                return barrel < 2 ? 0f : (rank * 2 >= barrels - 1 ? SideAngle : -SideAngle);

            default:
                return 0f;
        }
    }

    private static float Spread(int rank, int barrels, float step)
    {
        if (barrels <= 1)
            return 0f;

        return (rank - (barrels - 1) * 0.5f) * step;
    }
}
