using UnityEngine;

public class TorpedoDelivery : ProjectileDelivery
{
    private readonly Collider2D[] _near = new Collider2D[8];

    private SpawnPool<BlastWave> _blastPool;

    public TorpedoDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
    }

    protected override void OnTick(float deltaTime)
    {
        AmmoPool.ForEachActive(Watch);
    }

    private void Watch(Ammo torpedo)
    {
        if (torpedo.IsArmed == false)
            return;

        TorpedoWarhead warhead = torpedo.GetComponent<TorpedoWarhead>();

        if (warhead == null)
            return;

        Vector2 centre = torpedo.transform.position;

        if (ScreenArea.Contains(Context.Camera, centre) == false)
            return;

        if (Physics2D.OverlapCircle(centre, Row.Area, Context.TargetFilter, _near) == 0)
            return;

        Blast(warhead, centre);
        torpedo.Remove();
    }

    private void Blast(TorpedoWarhead warhead, Vector2 centre)
    {
        float radius = warhead.FarDiameter * 0.5f;
        int count = Physics2D.OverlapCircle(centre, radius, Context.TargetFilter, _near);

        for (int i = 0; i < count; i++)
        {
            if (_near[i].TryGetComponent(out IDamageable damageable) == false)
                damageable = _near[i].GetComponentInParent<IDamageable>();

            if (damageable != null)
                damageable.TakeDamage(Row.Damage);
        }

        Show(warhead, centre);
    }

    private void Show(TorpedoWarhead warhead, Vector2 centre)
    {
        if (warhead.Blast == null)
            return;

        if (_blastPool == null)
            _blastPool = Context.Pools.GetPool(warhead.Blast);

        BlastWave wave = _blastPool.Get(centre);

        wave.Play(warhead.NearDiameter, warhead.FarDiameter);
    }
}
