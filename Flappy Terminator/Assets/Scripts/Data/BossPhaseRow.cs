using System;
using UnityEngine;

[Serializable]
public class BossPhaseRow
{
    public const int FollowingPhase = 0;

    [SerializeField] private int _phase;
    [SerializeField] private string _weapon;
    [SerializeField] private int _barrels;
    [SerializeField] private int _damage;
    [SerializeField] private float _rateScale;
    [SerializeField] private PhaseExitKind _exit;
    [SerializeField] private float _exitValue;
    [SerializeField] private int _next;
    [SerializeField] private float _escapeHealth;
    [SerializeField] private int _escapeTo;

    public BossPhaseRow(int phase, string weapon, int barrels, int damage, float rateScale, PhaseExitKind exit,
        float exitValue, int next, float escapeHealth, int escapeTo)
    {
        _escapeHealth = escapeHealth;
        _escapeTo = escapeTo;
        _phase = phase;
        _weapon = weapon;
        _barrels = barrels;
        _damage = damage;
        _rateScale = rateScale;
        _exit = exit;
        _exitValue = exitValue;
        _next = next;
    }

    public int Phase => _phase;

    public string Weapon => _weapon;

    public int Barrels => _barrels;

    public int Damage => _damage;

    public float RateScale => _rateScale;

    public PhaseExitKind Exit => _exit;

    public float ExitValue => _exitValue;

    public int Next => _next;

    public float EscapeHealth => _escapeHealth;

    public int EscapeTo => _escapeTo;
}
