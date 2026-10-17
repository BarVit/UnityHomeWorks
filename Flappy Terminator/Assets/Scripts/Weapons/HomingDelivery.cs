using UnityEngine;

public class HomingDelivery : ProjectileDelivery
{
    private const float SearchRadius = 30f;

    private readonly Collider2D[] _candidates = new Collider2D[16];

    public HomingDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
    }

    protected override void OnTick(float deltaTime)
    {
        AmmoPool.ForEachActive(ammo => Steer(ammo, deltaTime));
    }

    private void Steer(Ammo ammo, float deltaTime)
    {
        Transform target = FindTarget(ammo.transform.position);

        if (target == null)
            return;

        Vector2 direction = (Vector2)(target.position - ammo.transform.position);

        if (direction.sqrMagnitude < Mathf.Epsilon)
            return;

        float wanted = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float current = ammo.transform.eulerAngles.z;
        float turned = Mathf.MoveTowardsAngle(current, wanted, Row.TurnRate * Context.GetBoost(ammo.Barrel).TurnScale * deltaTime);

        ammo.transform.rotation = Quaternion.Euler(0f, 0f, turned);
        ammo.Fly(ProjectileSpeed);
    }

    private Transform FindTarget(Vector3 from)
    {
        int count = Physics2D.OverlapCircle(from, SearchRadius, Context.TargetFilter, _candidates);
        Transform nearest = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (IsOpenTarget(_candidates[i]) == false)
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
