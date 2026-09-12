using System.Collections;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private Ammo _prefab;
    [SerializeField] private float _shootDelay;
    [SerializeField] private AudioSource _shotAudioSource;

    private Camera _camera;
    private SpawnPool<Ammo> _ammoPool;
    private SpawnPool<ExplosionAnimation> _hitEffectPool;
    private Coroutine _shooting;

    public Ammo AmmoPrefab => _prefab;

    private void Awake()
    {
        _camera = Camera.main;
    }

    public void Init(SpawnPool<Ammo> ammoPool, SpawnPool<ExplosionAnimation> hitEffectPool)
    {
        _ammoPool = ammoPool;
        _hitEffectPool = hitEffectPool;
    }

    public void StartShoot()
    {
        StopShoot();

        _shooting = StartCoroutine(Shoot());
    }

    public void StopShoot()
    {
        if (_shooting == null)
            return;

        StopCoroutine(_shooting);
        _shooting = null;
    }

    private IEnumerator Shoot()
    {
        WaitForSeconds shootDelay = new(_shootDelay);

        while (enabled)
        {
            Ammo ammo = _ammoPool.Get(_shootPoint.position, _shootPoint.rotation);

            ammo.Init(_hitEffectPool);
            if (ScreenArea.Contains(_camera, _shootPoint.position))
                _shotAudioSource.PlayOneShot(ammo.ShotSound.Clip, ammo.ShotSound.Volume);
            ammo.Fly();

            yield return shootDelay;
        }
    }
}
