public class HealthPhaseExit : PhaseExit
{
    private readonly float _share;

    public HealthPhaseExit(float share)
    {
        _share = share;
    }

    public override bool IsDone(float healthShare)
    {
        return healthShare <= _share;
    }
}
