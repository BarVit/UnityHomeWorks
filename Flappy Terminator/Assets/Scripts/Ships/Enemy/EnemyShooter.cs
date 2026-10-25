using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    private const string AmmoLayerName = "EnemyAmmo";
    private const string LayoutPrefix = "Layout ";

    [SerializeField] private Transform _mountRoot;
    [SerializeField] private AudioSource _shotAudioSource;

    private Transform[] _mounts;
    private ShotLoop _loop;
    private ShotLoop _hitLoop;
    private int _ammoLayer;
    private WeaponDelivery _delivery;
    private WeaponDelivery _special;
    private int _specialEvery;
    private int _attacks;
    private bool _isShooting;
    private bool _isFrozen;

    public event Action Fired;

    private void Awake()
    {
        _mounts = CollectMounts(_mountRoot);
        _loop = new ShotLoop(CreateLoopSource());
        _hitLoop = new ShotLoop(CreateLoopSource());
        _ammoLayer = LayerMask.NameToLayer(AmmoLayerName);
    }

    private void Update()
    {
        if (_delivery == null)
            return;

        if (_isFrozen)
        {
            FadeLoops(Time.deltaTime);

            return;
        }

        _delivery.Tick(Time.deltaTime);
        _special?.Tick(Time.deltaTime);

        int shots = _isShooting ? TryShoot() : 0;

        for (int shot = 0; shot < shots; shot++)
            Fired?.Invoke();
    }

    public void Arm(EnemyArmament armament, EnemyContext context)
    {
        Arm(armament, context, 0f);
    }

    public void Rearm(EnemyArmament armament, EnemyContext context)
    {
        Arm(armament, context, _delivery == null ? 0f : _delivery.NextShotIn);
    }

    public void ArmSpecial(EnemyArmament armament, int every, EnemyContext context)
    {
        _special?.Dispose();
        _special = null;
        _specialEvery = 0;
        _attacks = 0;

        if (armament == null || every <= 0)
            return;

        _special = WeaponFactory.Create(armament.Weapon, CreateContext(armament, context));
        _specialEvery = every;
    }

    public void StartShoot()
    {
        _isShooting = true;
    }

    public void StopShoot()
    {
        _isShooting = false;
        _isFrozen = false;
    }

    public void Freeze(bool isFrozen)
    {
        _isFrozen = isFrozen;
    }

    private void FadeLoops(float deltaTime)
    {
        _loop.Tick(deltaTime);
        _hitLoop.Tick(deltaTime);
    }

    private int TryShoot()
    {
        bool isAttackStart = _delivery.IsAttackStart;

        if (_special != null && isAttackStart && (_attacks + 1) % _specialEvery == 0)
        {
            int shots = _delivery.PassAttack();

            if (shots == 0)
                return 0;

            _special.FireOutOfTurn();
            _attacks++;

            return shots;
        }

        if (_delivery.Fire() == false)
            return 0;

        if (isAttackStart)
            _attacks++;

        return 1;
    }

    private void Arm(EnemyArmament armament, EnemyContext context, float cooldownLeft)
    {
        if (_delivery != null)
            _delivery.Dispose();

        _delivery = WeaponFactory.Create(armament.Weapon, CreateContext(armament, context));
        _delivery.ContinueCooldown(cooldownLeft);
    }

    private WeaponContext CreateContext(EnemyArmament armament, EnemyContext context)
    {
        return new WeaponContext(GetMounts(armament.Barrels), armament.Barrels, context.Pools,
            new ShotAudio(_shotAudioSource, context.Camera), context.HitAudio, context.TargetLayers,
            _ammoLayer, context.Camera, default, _loop, _hitLoop, armament.Damage,
            rateScale: armament.RateScale);
    }

    private AudioSource CreateLoopSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();

        source.outputAudioMixerGroup = _shotAudioSource.outputAudioMixerGroup;
        source.spatialBlend = _shotAudioSource.spatialBlend;
        source.playOnAwake = false;

        return source;
    }

    private Transform[] GetMounts(int barrels)
    {
        Transform layout = _mountRoot.Find(LayoutPrefix + barrels);

        return layout == null ? _mounts : CollectMounts(layout);
    }

    private static Transform[] CollectMounts(Transform parent)
    {
        List<Transform> mounts = new List<Transform>();

        foreach (Transform child in parent)
        {
            if (child.name.StartsWith(LayoutPrefix) == false)
                mounts.Add(child);
        }

        return mounts.ToArray();
    }
}
