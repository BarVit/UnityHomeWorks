using UnityEngine;

public static class FireSpread
{
    private const int WingBarrels = 4;

    public static float GetAngle(FirePattern pattern, int rank, int barrels, float wingAngle)
    {
        switch (pattern)
        {
            case FirePattern.Wings:
                return Wing(rank, barrels, wingAngle);

            default:
                return 0f;
        }
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
