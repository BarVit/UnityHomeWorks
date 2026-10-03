using System;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private WeaponTable _weapons;
    [SerializeField] private string _weaponId;
    [SerializeField] private int _barrels = 1;
    [SerializeField] private Transform _mountRoot;
    [SerializeField] private AudioSource _shotAudioSource;
    [SerializeField] private AudioSource _hitAudioSource;
    [SerializeField] private AudioSource _loopAudioSource;
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private LayerMask _worldLayers;

    private readonly PoolRegistry _pools = new PoolRegistry();

    private Transform[] _mounts;
    private WeaponDelivery _delivery;
    private float _shownReadiness = -1f;

    public event Action<float> ReadinessChanged;

    public string WeaponId => _weaponId;

    public int Barrels => _barrels;

    public Transform LockTarget => _delivery == null ? null : _delivery.LockTarget;

    public bool IsOverheated => _delivery != null && _delivery.IsOverheated;

    public bool HasHeat => _delivery != null && _delivery.HasHeat;

    public float Readiness => _delivery == null ? 0f : _delivery.Readiness;

    public bool IsReady => _delivery != null && _delivery.IsReady;

    private void Awake()
    {
        _mounts = CollectMounts();

        WeaponRow row = _weapons.GetRow(_weaponId);

        if (row == null)
            throw new InvalidOperationException(
                "PlayerShooter: в таблице оружия нет строки с id " + _weaponId);

        Camera camera = Camera.main;
        WeaponContext context = new WeaponContext(_mounts, _barrels, _pools,
            new ShotAudio(_shotAudioSource, camera), new ShotAudio(_hitAudioSource, camera),
            _targetLayers, camera, 1f, _worldLayers, new ShotLoop(_loopAudioSource));

        _delivery = WeaponFactory.Create(row, context);
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
