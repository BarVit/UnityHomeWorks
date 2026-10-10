public class TimePhaseExit : PhaseExit
{
    private float _timeLeft;

    public TimePhaseExit(float duration)
    {
        _timeLeft = duration;
    }

    public override void Tick(float deltaTime)
    {
        _timeLeft -= deltaTime;
    }

    public override bool IsDone(float healthShare)
    {
        return _timeLeft <= 0f;
    }
}
