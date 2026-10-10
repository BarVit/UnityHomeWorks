using System;
using System.Collections.Generic;

public class BossPhaseRunner
{
    public const int NoPhase = -1;

    private readonly IReadOnlyList<BossPhase> _phases;
    private readonly Health _health;
    private readonly EnemyShooter _shooter;
    private readonly EnemyContext _context;

    private PhaseExit _exit;

    public BossPhaseRunner(IReadOnlyList<BossPhase> phases, Health health, EnemyShooter shooter,
        EnemyContext context)
    {
        _phases = phases;
        _health = health;
        _shooter = shooter;
        _context = context;
    }

    public int Current { get; private set; } = NoPhase;

    public event Action<int> PhaseChanged;

    private float HealthShare => (float)_health.Value / _health.MaxValue;

    public void Start()
    {
        _shooter.Fired += OnFired;
        Begin(0);
        _shooter.Arm(_phases[0].Armament, _context);
    }

    public void Stop()
    {
        _shooter.Fired -= OnFired;
        Current = NoPhase;
    }

    public void Tick(float deltaTime)
    {
        if (Current == NoPhase)
            return;

        _exit.Tick(deltaTime);
        Advance();
    }

    private void OnFired()
    {
        _exit.CountShot();
        Advance();
    }

    private void Advance()
    {
        for (int step = 0; step < _phases.Count && _exit.IsDone(HealthShare); step++)
        {
            int next = _phases[Current].Next;

            if (next == BossPhase.Stay)
            {
                _exit = new PhaseExit();

                return;
            }

            Begin(next);
            _shooter.Rearm(_phases[next].Armament, _context);
        }
    }

    private void Begin(int index)
    {
        Current = index;
        _exit = _phases[index].CreateExit();
        PhaseChanged?.Invoke(index);
    }
}
