using System;
using System.Collections;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private Ammo _prefab;
    [SerializeField] private float _shootRate;
    [SerializeField] private AudioSource _shotAudioSource;

    private SpawnPool<Ammo> _ammoPool;
    private SpawnPool<ExplosionAnimation> _hitEffectPool;
    private bool _isOnCooldown;

    public event Action<float> ReadinessChanged;

    private void Awake()
    {
        _ammoPool = new SpawnPool<Ammo>(_prefab);
        _hitEffectPool = new SpawnPool<ExplosionAnimation>(_prefab.HitEffectPrefab);
    }

    public void ReleaseAll()
    {
        _ammoPool.ReleaseAll();
        _hitEffectPool.ReleaseAll();
    }

    public void Shoot()
    {
        if (_isOnCooldown)
            return;

        Ammo ammo = _ammoPool.Get(_shootPoint.position, _shootPoint.rotation);

        ammo.Init(_hitEffectPool);
        _shotAudioSource.PlayOneShot(ammo.ShotSound.Clip, ammo.ShotSound.Volume);
        ammo.Fly();

        _isOnCooldown = true;
        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        for (float elapsed = 0f; elapsed < _shootRate; elapsed += Time.deltaTime)
        {
            ReadinessChanged?.Invoke(elapsed / _shootRate);

            yield return null;
        }

        _isOnCooldown = false;
        ReadinessChanged?.Invoke(1f);
    }
}
