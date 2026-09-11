using UnityEngine;
using System.Collections;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private Ammo _prefab;
    [SerializeField] private float _shootDelay;

    private ObjectPoolAmmo _pool;
    private Coroutine _shooter;

    private void Awake()
    {
        _pool = new(_prefab);
    }

    public void StartShoot()
    {
        _shooter = StartCoroutine(Shoot());
    }

    public void StopShoot()
    {
        StopCoroutine(Shoot());
    }

    private IEnumerator Shoot()
    {
        WaitForSeconds shootDelay = new(_shootDelay);

        while(enabled)
        {
            Ammo ammo = _pool.Get();

            ammo.transform.position = _shootPoint.transform.position;
            ammo.transform.rotation = _shootPoint.transform.rotation;
            ammo.PlayShotSound();

            if (ammo.TryGetComponent(out BulletMover bulletMover))
                bulletMover.Fly();

            yield return shootDelay;
        }
    }
}