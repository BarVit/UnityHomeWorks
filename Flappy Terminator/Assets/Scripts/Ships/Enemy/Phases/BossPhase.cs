public class BossPhase
{
    public const int Stay = -1;

    private readonly PhaseExitKind _exit;
    private readonly float _exitValue;
    private readonly float _escapeHealth;

    public BossPhase(EnemyArmament armament, PhaseExitKind exit, float exitValue, int next, float escapeHealth,
        int escapeTo)
    {
        _escapeHealth = escapeHealth;
        EscapeTo = escapeTo;
        Armament = armament;
        Next = next;
        _exit = exit;
        _exitValue = exitValue;
    }

    public EnemyArmament Armament { get; private set; }

    public int Next { get; private set; }

    public int EscapeTo { get; private set; }

    public bool ShouldEscape(float healthShare)
    {
        return _escapeHealth > 0f && healthShare <= _escapeHealth;
    }

    public PhaseExit CreateExit()
    {
        switch (_exit)
        {
            case PhaseExitKind.Health:
                return new HealthPhaseExit(_exitValue);
            case PhaseExitKind.Time:
                return new TimePhaseExit(_exitValue);
            case PhaseExitKind.Shots:
                return new ShotsPhaseExit((int)_exitValue);
            default:
                return new PhaseExit();
        }
    }
}
