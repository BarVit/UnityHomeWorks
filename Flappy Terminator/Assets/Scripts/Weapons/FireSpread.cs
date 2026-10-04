using UnityEngine;

public static class FireSpread
{
    private const float ConeStep = 12f;
    private const float NarrowConeStep = 4f;
    private const float SideAngle = 60f;
    private const int WingBarrels = 4;

    public static float GetAngle(FirePattern pattern, int barrel, int rank, int barrels, float wingAngle)
    {
        switch (pattern)
        {
            case FirePattern.Cone:
                return Spread(rank, barrels, ConeStep);

            case FirePattern.NarrowCone:
                return Spread(rank, barrels, NarrowConeStep);

            case FirePattern.Wings:
                return Wing(rank, barrels, wingAngle);

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

    private static float Wing(int rank, int barrels, float wingAngle)
    {
        if (barrels < WingBarrels)
            return 0f;

        if (rank == 0)
            return -wingAngle;

        return rank == barrels - 1 ? wingAngle : 0f;
    }
}
