using System;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    private const string AmmoLayerName = "EnemyAmmo";

    [SerializeField] private Transform _mountRoot;
    [SerializeField] private AudioSource _shotAudioSource;

    private Transform[] _mounts;
    private ShotLoop _loop;
    private ShotLoop _hitLoop;
    private int _ammoLayer;
    private WeaponDelivery _delivery;
    private bool _isShooting;

    public event Action Fired;

    private void Awake()
    {
        _mounts = CollectMounts();
        _loop = new ShotLoop(CreateLoopSource());
        _hitLoop = new ShotLoop(CreateLoopSource());
        _ammoLayer = LayerMask.NameToLayer(AmmoLayerName);
    }

    private void Update()
    {
        if (_delivery == null)
            return;

        _delivery.Tick(Time.deltaTime);

        if (_isShooting && _delivery.Fire())
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

    public void StartShoot()
    {
        _isShooting = true;
    }

    public void StopShoot()
    {
        _isShooting = false;
    }

    private void Arm(EnemyArmament armament, EnemyContext context, float cooldownLeft)
    {
        WeaponContext weapon = new WeaponContext(_mounts, armament.Barrels, context.Pools,
            new ShotAudio(_shotAudioSource, context.Camera), context.HitAudio, context.TargetLayers,
            _ammoLayer, context.Camera, default, _loop, _hitLoop, armament.Damage,
            rateScale: armament.RateScale);

        if (_delivery != null)
            _delivery.Dispose();

        _delivery = WeaponFactory.Create(armament.Weapon, weapon);
        _delivery.ContinueCooldown(cooldownLeft);
    }

    private AudioSource CreateLoopSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();

        source.outputAudioMixerGroup = _shotAudioSource.outputAudioMixerGroup;
        source.spatialBlend = _shotAudioSource.spatialBlend;
        source.playOnAwake = false;

        return source;
    }

    private Transform[] CollectMounts()
    {
        Transform[] mounts = new Transform[_mountRoot.childCount];

        for (int i = 0; i < mounts.Length; i++)
            mounts[i] = _mountRoot.GetChild(i);

        return mounts;
    }
}
