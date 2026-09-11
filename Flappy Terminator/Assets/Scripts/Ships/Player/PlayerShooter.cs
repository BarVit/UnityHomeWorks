using UnityEngine;
using System.Collections;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private Ammo _prefab;
    [SerializeField] private float _shootRate;

    private ObjectPoolAmmo _pool;
    private Coroutine _shootDelayer;
    private bool _isOnCooldown;

    private void Start()
    {
        _pool = new(_prefab);
    }

    public void Shoot()
    {
        if(_isOnCooldown == false)
        {
            Ammo ammo = _pool.Get();

            ammo.transform.position = _shootPoint.transform.position;
            ammo.transform.rotation = _shootPoint.transform.rotation;
            ammo.PlayShotSound();

            if (ammo.TryGetComponent(out BulletMover bulletMover))
                bulletMover.Fly();

            _isOnCooldown = true;
            _shootDelayer = StartCoroutine(Delay());
        }
    }

    private IEnumerator Delay()
    {
        WaitForSeconds wait = new(_shootRate);

        yield return wait;

        _isOnCooldown = false;
    }
}