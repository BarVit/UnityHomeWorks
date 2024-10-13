using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletShooter : MonoBehaviour
{
    [SerializeField] private Rigidbody _prefab;
    [SerializeField] private float _speed;
    [SerializeField] private float _cooldown;
    
    [field : SerializeField] public Transform Target {get; private set; }

    private void Start()
    {
           StartCoroutine(Shoot());
    }

    public void SetTarget(Transform target)
    {
        Target = target;
    }

    private IEnumerator Shoot()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(_cooldown);

        while (true)
        {
            Vector3 direction = (Target.position - transform.position).normalized;

            Rigidbody bullet = Instantiate(_prefab, transform.position, Quaternion.identity);

            bullet.transform.forward = direction;
            bullet.velocity = direction * _speed;
            yield return waitForSeconds;
         }
    }
}