public class ShotsPhaseExit : PhaseExit
{
    private int _shotsLeft;

    public ShotsPhaseExit(int shots)
    {
        _shotsLeft = shots;
    }

    public override void CountShot()
    {
        _shotsLeft--;
    }

    public override bool IsDone(float healthShare)
    {
        return _shotsLeft <= 0;
    }
}
