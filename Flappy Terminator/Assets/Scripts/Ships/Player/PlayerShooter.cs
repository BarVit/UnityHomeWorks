using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    private const string AmmoLayerName = "PlayerAmmo";

    [SerializeField] private WeaponTable _weapons;
    [SerializeField] private string _weaponId;
    [SerializeField] private int _barrels = 1;
    [SerializeField] private Transform[] _layouts;
    [SerializeField] private AudioSource _shotAudioSource;
    [SerializeField] private AudioSource _hitAudioSource;
    [SerializeField] private AudioSource _loopAudioSource;
    [SerializeField] private AudioSource _hitLoopAudioSource;
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private LayerMask _worldLayers;
    [SerializeField] private CritMark _critMark;
    [SerializeField] private ParticleSystem _burnFlame;

    private readonly PoolRegistry _pools = new PoolRegistry("Player Weapon");
    private readonly ShotTally _tally = new ShotTally();

    private Transform[] _mounts;
    private int _equippedBarrels = 1;
    private WeaponDelivery _delivery;
    private float _targetHoldTime;
    private string _equippedId;
    private AudioClip _overheatClip;
    private float _shownReadiness = -1f;
    private int _shownShots = -1;

    public event Action<float> ReadinessChanged;

    public event Action<int> ShotsLeftChanged;

    public event Action WeaponChanged;

    public ShotTally Tally => _tally;

    public string WeaponId => _equippedId;

    public string TestWeaponId => _weaponId;

    public int Barrels => _equippedBarrels;

    public IReadOnlyList<Transform> Mounts => _mounts;

    public Transform LockTarget => _delivery == null ? null : _delivery.LockTarget;

    public float TargetHoldTime => _targetHoldTime;

    public bool IsOverheated => _delivery != null && _delivery.IsOverheated;

    public AudioClip OverheatClip => _overheatClip;

    public bool HasHeat => _delivery != null && _delivery.HasHeat;

    public float Readiness => _delivery == null ? 0f : _delivery.Readiness;

    public bool IsReady => _delivery != null && _delivery.IsReady;

    public bool HasMagazine => _delivery != null && _delivery.HasMagazine;

    public int ShotsLeft => _delivery == null ? 0 : _delivery.ShotsLeft;

    public int MagazineSize => _delivery == null ? 0 : _delivery.MagazineSize;

    private void Awake()
    {
        EquipDefault();
    }

    private void Update()
    {
        _delivery.Tick(Time.deltaTime);
        Show(_delivery.Readiness);
        ShowShots(_delivery.ShotsLeft);
    }

    public void Shoot()
    {
        _delivery.Fire();
    }

    public void ReleaseAll()
    {
        _pools.ReleaseAll();
    }


    public void Equip(string weaponId, WeaponBoost[] boosts, WeaponSkills skills)
    {
        WeaponRow row = _weapons.GetRow(weaponId);

        if (row == null)
            throw new InvalidOperationException(
                "PlayerShooter: в таблице оружия нет строки с id " + weaponId);

        if (_delivery != null)
        {
            _delivery.Dispose();
            _pools.ReleaseAll();
        }

        _equippedBarrels = Mathf.Max(1, boosts.Length);
        _mounts = CollectMounts(GetLayout(row));

        Camera camera = Camera.main;
        WeaponContext context = new WeaponContext(_mounts, _equippedBarrels, _pools,
            new ShotAudio(_shotAudioSource, camera), new ShotAudio(_hitAudioSource, camera),
            _targetLayers, LayerMask.NameToLayer(AmmoLayerName), camera, _worldLayers, new ShotLoop(_loopAudioSource),
            new ShotLoop(_hitLoopAudioSource), boosts: boosts, critMark: _critMark,
            tally: _tally, rateScale: skills.RateScale, burnFlame: _burnFlame, reloadShift: skills.ReloadShift,
            magazineBonus: skills.MagazineBonus, speedShift: skills.SpeedShift, hasSight: skills.HasSight,
            heatPoolScale: skills.HeatPoolScale, coolingTimeScale: skills.CoolingTimeScale,
            damageScale: skills.DamageScale);

        _equippedId = weaponId;
        _targetHoldTime = boosts.Length > 0 ? boosts[0].TargetHoldSeconds : 0f;
        _delivery = WeaponFactory.Create(row, context);
        _overheatClip = row.Assets == null ? null : row.Assets.OverheatSound;
        _shownReadiness = -1f;
        _shownShots = -1;
        WeaponChanged?.Invoke();
    }

    private void EquipDefault()
    {
        Equip(_weaponId, new WeaponBoost[Mathf.Max(1, _barrels)], WeaponSkills.Default);
    }

    private Transform GetLayout(WeaponRow row)
    {
        int barrels = Mathf.Clamp(_equippedBarrels, 1, Mathf.Min(row.BarrelsMax, _layouts.Length));

        return _layouts[barrels - 1];
    }

    private Transform[] CollectMounts(Transform layout)
    {
        Transform[] mounts = new Transform[layout.childCount];

        for (int i = 0; i < mounts.Length; i++)
            mounts[i] = layout.GetChild(i);

        return mounts;
    }

    private void ShowShots(int shotsLeft)
    {
        if (shotsLeft == _shownShots)
            return;

        _shownShots = shotsLeft;
        ShotsLeftChanged?.Invoke(shotsLeft);
    }

    private void Show(float readiness)
    {
        if (Mathf.Approximately(readiness, _shownReadiness))
            return;

        _shownReadiness = readiness;
        ReadinessChanged?.Invoke(readiness);
    }
}
