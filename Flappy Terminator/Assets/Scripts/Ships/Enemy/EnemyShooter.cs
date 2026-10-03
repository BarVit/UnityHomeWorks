using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private Transform _mountRoot;
    [SerializeField] private AudioSource _shotAudioSource;

    private Transform[] _mounts;
    private WeaponDelivery _delivery;
    private bool _isShooting;

    private void Awake()
    {
        _mounts = CollectMounts();
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
            context.Camera, setup.ProjectileSpeedScale);

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

    private Transform[] CollectMounts()
    {
        Transform[] mounts = new Transform[_mountRoot.childCount];

        for (int i = 0; i < mounts.Length; i++)
            mounts[i] = _mountRoot.GetChild(i);

        return mounts;
    }
}
