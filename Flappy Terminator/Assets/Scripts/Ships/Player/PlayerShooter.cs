using System;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private WeaponTable _weapons;
    [SerializeField] private string _weaponId;
    [SerializeField] private int _barrels = 1;
    [SerializeField] private Transform _mountRoot;
    [SerializeField] private AudioSource _shotAudioSource;

    private readonly PoolRegistry _pools = new PoolRegistry();

    private Transform[] _mounts;
    private WeaponDelivery _delivery;
    private float _shownReadiness = -1f;

    public event Action<float> ReadinessChanged;

    public string WeaponId => _weaponId;

    public int Barrels => _barrels;

    private void Awake()
    {
        _mounts = CollectMounts();

        WeaponRow row = _weapons.GetRow(_weaponId);

        if (row == null)
            throw new InvalidOperationException(
                "PlayerShooter: в таблице оружия нет строки с id " + _weaponId);

        _delivery = WeaponFactory.Create(row, _mounts, _barrels, _pools,
            new ShotAudio(_shotAudioSource, Camera.main));
    }

    private void Update()
    {
        _delivery.Tick(Time.deltaTime);
        Show(_delivery.Readiness);
    }

    public void Shoot()
    {
        _delivery.Fire();
    }

    public void ReleaseAll()
    {
        _pools.ReleaseAll();
    }

    private Transform[] CollectMounts()
    {
        Transform[] mounts = new Transform[_mountRoot.childCount];

        for (int i = 0; i < mounts.Length; i++)
            mounts[i] = _mountRoot.GetChild(i);

        return mounts;
    }

    private void Show(float readiness)
    {
        if (Mathf.Approximately(readiness, _shownReadiness))
            return;

        _shownReadiness = readiness;
        ReadinessChanged?.Invoke(readiness);
    }
}
