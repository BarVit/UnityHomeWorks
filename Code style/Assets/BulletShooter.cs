using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletShooter : MonoBehaviour
{
    [SerializeField] Rigidbody _prefab;
    [SerializeField] private float _speed;
    [SerializeField] private float _timeWaitShooting;

    [field : SerializeField] public Transform Target;

    private void Start()
    {
        StartCoroutine(Shoot());
    }

    private IEnumerator Shoot()
    {
        while (true)
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(_timeWaitShooting);

            Vector3 direction = (Target.position - transform.position).normalized;

            Rigidbody bullet = Instantiate(_prefab, transform.position + direction, Quaternion.identity);

            bullet.transform.up = direction;
            bullet.velocity = direction * _speed;
            yield return waitForSeconds;
         }
    }
}