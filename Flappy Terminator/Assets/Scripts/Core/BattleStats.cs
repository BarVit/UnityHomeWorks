using System;
using UnityEngine;

public class BattleStats : MonoBehaviour
{
    private const int MaxStars = 3;

    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private PlayerShip _playerShip;
    [SerializeField] private PlayerShooter _shooter;
    [SerializeField] private Economy _economy;
    [SerializeField] private float _noDamageShare = 0.1f;
    [SerializeField] private float _noMissShare = 0.05f;

    private int _levelDamage;
    private int _levelFired;
    private int _levelHit;

    public int TotalEnemies { get; private set; }

    public int Killed { get; private set; }

    public int Fired { get; private set; }

    public int Hit { get; private set; }

    public int DamageTaken { get; private set; }

    public int EnvironmentDamage { get; private set; }

    public Reward NoDamageBonus { get; private set; }

    public Reward NoMissBonus { get; private set; }

    public Reward Bonus => NoDamageBonus + NoMissBonus;

    public int Accuracy => Fired == 0 ? 100 : Mathf.FloorToInt(100f * Hit / Fired);

    public int Stars => 1 + (Killed >= TotalEnemies ? 1 : 0) + (EnvironmentDamage == 0 ? 1 : 0);

    public int MaxStarCount => MaxStars;

    public event Action Changed;

    private void OnEnable()
    {
        _enemySpawner.EnemyDied += OnEnemyDied;
        _playerShip.Damaged += OnDamaged;
        _shooter.Tally.Fired += OnFired;
        _shooter.Tally.Hit += OnHit;
    }

    private void OnDisable()
    {
        _enemySpawner.EnemyDied -= OnEnemyDied;
        _playerShip.Damaged -= OnDamaged;
        _shooter.Tally.Fired -= OnFired;
        _shooter.Tally.Hit -= OnHit;
    }

    public void BeginLocation(int totalEnemies)
    {
        TotalEnemies = totalEnemies;
        Killed = 0;
        Fired = 0;
        Hit = 0;
        DamageTaken = 0;
        EnvironmentDamage = 0;
        NoDamageBonus = default;
        NoMissBonus = default;
        Changed?.Invoke();
    }

    public void BeginLevel()
    {
        _levelDamage = 0;
        _levelFired = 0;
        _levelHit = 0;
    }

    public void CompleteLevel(Reward levelReward)
    {
        if (_levelDamage == 0)
            NoDamageBonus += Share(levelReward, _noDamageShare);

        if (_levelHit >= _levelFired)
            NoMissBonus += Share(levelReward, _noMissShare);
    }

    public void PayBonus()
    {
        _economy.Grant(Bonus);
    }

    private static Reward Share(Reward reward, float share)
    {
        return new Reward(Mathf.FloorToInt(reward.Xp * share + 0.5f), Mathf.FloorToInt(reward.Money * share + 0.5f));
    }

    private void OnEnemyDied(EnemyShip ship)
    {
        Killed++;
        Changed?.Invoke();
    }

    private void OnDamaged(int amount, bool isEnvironment)
    {
        DamageTaken += amount;
        _levelDamage += amount;

        if (isEnvironment)
            EnvironmentDamage += amount;
    }

    private void OnFired()
    {
        Fired++;
        _levelFired++;
    }

    private void OnHit()
    {
        Hit++;
        _levelHit++;
    }
}
