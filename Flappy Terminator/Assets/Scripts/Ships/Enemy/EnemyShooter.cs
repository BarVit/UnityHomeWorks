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

        if (_isShooting)
            _delivery.Fire();
    }

    public void Arm(EnemySetup setup, EnemyContext context)
    {
        WeaponContext weapon = new WeaponContext(_mounts, setup.Barrels, context.Pools,
            new ShotAudio(_shotAudioSource, context.Camera), context.HitAudio, context.TargetLayers,
            _ammoLayer, context.Camera, default, _loop, _hitLoop, setup.Damage);

        if (_delivery != null)
            _delivery.Dispose();

        _delivery = WeaponFactory.Create(setup.Weapon, weapon);
    }

    public void StartShoot()
    {
        _isShooting = true;
    }

    public void StopShoot()
    {
        _isShooting = false;
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
