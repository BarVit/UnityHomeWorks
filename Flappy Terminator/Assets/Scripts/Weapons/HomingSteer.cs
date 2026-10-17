using UnityEngine;

[RequireComponent(typeof(Ammo))]
public class HomingSteer : MonoBehaviour
{
    private const float SearchRadius = 30f;

    private readonly Collider2D[] _candidates = new Collider2D[16];

    private Ammo _ammo;
    private ContactFilter2D _targets;
    private float _turnRate;
    private float _speed;

    public static void Guide(Ammo ammo, float turnRate, float speed, ContactFilter2D targets)
    {
        if (ammo.TryGetComponent(out HomingSteer steer) == false)
            steer = ammo.gameObject.AddComponent<HomingSteer>();

        steer.Begin(turnRate, speed, targets);
    }

    private void Awake()
    {
        _ammo = GetComponent<Ammo>();
    }

    private void OnDisable()
    {
        enabled = false;
    }

    private void Update()
    {
        if (_ammo.IsArmed == false)
            return;

        Transform target = FindTarget(transform.position);

        if (target == null)
            return;

        Vector2 direction = target.position - transform.position;

        if (direction.sqrMagnitude < Mathf.Epsilon)
            return;

        float wanted = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float turned = Mathf.MoveTowardsAngle(transform.eulerAngles.z, wanted, _turnRate * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0f, 0f, turned);
        _ammo.Fly(_speed);
    }

    private void Begin(float turnRate, float speed, ContactFilter2D targets)
    {
        _turnRate = turnRate;
        _speed = speed;
        _targets = targets;
        enabled = true;
    }

    private Transform FindTarget(Vector3 from)
    {
        int count = Physics2D.OverlapCircle(from, SearchRadius, _targets, _candidates);
        Transform nearest = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (WeaponDelivery.IsOpenTarget(_candidates[i]) == false)
                continue;

            float distance = ((Vector2)(_candidates[i].transform.position - from)).sqrMagnitude;

            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearest = _candidates[i].transform;
        }

        return nearest;
    }
}
