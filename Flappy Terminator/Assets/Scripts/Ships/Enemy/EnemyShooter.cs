using System.Collections;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private Ammo _prefab;
    [SerializeField] private float _shootDelay;

    private SpawnPool<Ammo> _ammoPool;
    private SpawnPool<ExplosionAnimation> _hitEffectPool;
    private Coroutine _shooting;

    private void Awake()
    {
        _ammoPool = new SpawnPool<Ammo>(_prefab);
        _hitEffectPool = new SpawnPool<ExplosionAnimation>(_prefab.HitEffectPrefab);
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
            Ammo ammo = _ammoPool.Get();

            ammo.Init(_hitEffectPool);
            ammo.transform.SetPositionAndRotation(_shootPoint.position, _shootPoint.rotation);
            ammo.PlayShotSound();
            ammo.Fly();

            yield return shootDelay;
        }
    }
}
