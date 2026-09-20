using UnityEngine;

public class TorpedoDelivery : ProjectileDelivery
{
    private readonly Collider2D[] _near = new Collider2D[8];
    private readonly RaycastHit2D[] _ahead = new RaycastHit2D[1];

    private SpawnPool<BlastWave> _blastPool;
    private float _deltaTime;

    public TorpedoDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
    }

    protected override Ammo Launch(int barrel)
    {
        Ammo torpedo = base.Launch(barrel);

        torpedo.Init(null);
        torpedo.SetDamage(0);
        torpedo.Struck += OnStruck;

        if (torpedo.TryGetComponent(out TorpedoWarhead warhead))
            warhead.Cancel();

        return torpedo;
    }

    protected override void OnTick(float deltaTime)
    {
        _deltaTime = deltaTime;
        AmmoPool.ForEachActive(Watch);
    }

    private void Watch(Ammo torpedo)
    {
        if (torpedo.IsArmed == false)
            return;

        if (torpedo.TryGetComponent(out TorpedoWarhead warhead) == false)
            return;

        Vector2 centre = torpedo.transform.position;

        if (warhead.IsFuseBurning)
        {
            if (warhead.Burn(_deltaTime))
            {
                Blast(warhead, centre);
                torpedo.Remove();
            }

            return;
        }

        if (ScreenArea.Contains(Context.Camera, centre) == false)
            return;

        if (IsOnCourse(torpedo.transform, warhead))
            return;

        if (Physics2D.OverlapCircle(centre, Row.Area, Context.TargetFilter, _near) > 0)
            warhead.StartFuse();
    }

    private void OnStruck(Ammo torpedo, Vector2 point)
    {
        if (torpedo.TryGetComponent(out TorpedoWarhead warhead))
            Blast(warhead, point);
    }

    private bool IsOnCourse(Transform torpedo, TorpedoWarhead warhead)
    {
        Vector2 course = torpedo.right;
        Vector2 edge = new Vector2(-course.y, course.x) * (warhead.Width * 0.5f);
        Vector2 centre = torpedo.position;

        return SeesTarget(centre + edge, course) || SeesTarget(centre - edge, course);
    }

    private bool SeesTarget(Vector2 origin, Vector2 course)
    {
        return Physics2D.Raycast(origin, course, Context.TargetFilter, _ahead, Row.Area) > 0;
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

        ShowHit(centre);
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
