using System.Collections.Generic;
using UnityEngine;

public class HomingDelivery : ProjectileDelivery
{
    private const float SearchRadius = 30f;

    private readonly Collider2D[] _candidates = new Collider2D[32];
    private readonly List<Component> _targets = new List<Component>();
    private readonly List<int> _barrelOrder = new List<int>();
    private readonly Component[] _assigned;

    public HomingDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
        _assigned = new Component[Mathf.Max(1, context.Mounts.Length)];
    }

    protected override void OnFire()
    {
        AssignTargets();
        base.OnFire();
    }

    protected override Ammo Launch(int barrel)
    {
        Ammo ammo = base.Launch(barrel);
        WeaponBoost boost = Context.GetBoost(barrel);
        float turnRate = Row.TurnRate * boost.TurnScale;

        HomingSteer.Guide(ammo, turnRate, ProjectileSpeed * boost.SpeedScale, Context.TargetFilter,
            _assigned[barrel % _assigned.Length]);

        return ammo;
    }

    private void AssignTargets()
    {
        CollectTargets();
        SortBarrelsByHeight();

        for (int rank = 0; rank < _barrelOrder.Count; rank++)
        {
            int barrel = _barrelOrder[rank];

            _assigned[barrel % _assigned.Length] = _targets.Count == 0
                ? null
                : _targets[rank * _targets.Count / _barrelOrder.Count];
        }
    }

    private void CollectTargets()
    {
        _targets.Clear();

        Vector2 from = GetMount(0).position;
        int count = Physics2D.OverlapCircle(from, SearchRadius, Context.TargetFilter, _candidates);

        for (int i = 0; i < count; i++)
        {
            Component target = FindDamageable(_candidates[i]) as Component;

            if (target == null || _targets.Contains(target) || IsOpenTarget(_candidates[i]) == false)
                continue;

            if (ScreenArea.Contains(Context.Camera, target.transform.position))
                _targets.Add(target);
        }

        _targets.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
    }

    private void SortBarrelsByHeight()
    {
        _barrelOrder.Clear();

        for (int barrel = 0; barrel < Barrels; barrel++)
            _barrelOrder.Add(barrel);

        _barrelOrder.Sort((a, b) => GetMount(b).position.y.CompareTo(GetMount(a).position.y));
    }
}
