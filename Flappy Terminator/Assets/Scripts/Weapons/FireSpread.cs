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

            case FirePattern.Fan:
                return Fan(rank, barrels, wingAngle);

            default:
                return 0f;
        }
    }

    private static float Fan(int rank, int barrels, float edgeAngle)
    {
        if (barrels < 2)
            return 0f;

        return Mathf.Lerp(-edgeAngle, edgeAngle, rank / (float)(barrels - 1));
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
