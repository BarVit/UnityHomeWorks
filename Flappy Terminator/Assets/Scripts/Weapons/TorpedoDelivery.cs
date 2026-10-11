using UnityEngine;

public class TorpedoDelivery : ProjectileDelivery
{
    private readonly Collider2D[] _near = new Collider2D[8];
    private readonly RaycastHit2D[] _ahead = new RaycastHit2D[8];

    private SpawnPool<BlastWave> _blastPool;
    private float _deltaTime;

    public TorpedoDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
    }

    protected override Ammo Launch(int barrel)
    {
        Ammo torpedo = Spawn(barrel);

        LaunchSound launchSound = torpedo.GetComponentInChildren<LaunchSound>();

        if (barrel == 0 && launchSound != null)
            launchSound.Play(torpedo.ShotSound);

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

        Vector2 sensor = warhead.Sensor;

        if (warhead.IsFuseBurning)
        {
            if (warhead.Burn(_deltaTime))
            {
                if (torpedo.TryScore())
                    CountHit();

                Blast(warhead, torpedo.Barrel);
                torpedo.Remove();
            }

            return;
        }

        if (ScreenArea.Contains(Context.Camera, sensor) == false)
            return;

        if (IsOnCourse(torpedo.transform, warhead))
            return;

        int count = Physics2D.OverlapCircle(sensor, Row.Area, Context.TargetFilter, _near);

        for (int i = 0; i < count; i++)
        {
            if (IsOpenTarget(_near[i]))
            {
                warhead.StartFuse();

                return;
            }
        }
    }

    private void OnStruck(Ammo torpedo)
    {
        if (torpedo.TryGetComponent(out TorpedoWarhead warhead))
            Blast(warhead, torpedo.Barrel);
    }

    private bool IsOnCourse(Transform torpedo, TorpedoWarhead warhead)
    {
        Vector2 course = torpedo.right;
        Vector2 edge = new Vector2(-course.y, course.x) * (warhead.Width * 0.5f);
        Vector2 sensor = warhead.Sensor;

        return SeesTarget(sensor + edge, course) || SeesTarget(sensor - edge, course);
    }

    private bool SeesTarget(Vector2 origin, Vector2 course)
    {
        int count = Physics2D.Raycast(origin, course, Context.TargetFilter, _ahead, Row.Area);

        for (int i = 0; i < count; i++)
        {
            if (IsOpenTarget(_ahead[i].collider))
                return true;
        }

        return false;
    }

    private void Blast(TorpedoWarhead warhead, int barrel)
    {
        Vector2 centre = warhead.Centre;

        ShowHit(centre);
        Context.HitAudio.Play(warhead.BlastSound, centre);

        if (warhead.Blast == null)
            return;

        if (_blastPool == null)
            _blastPool = Context.Pools.GetPool(warhead.Blast);

        BlastWave wave = _blastPool.Get(centre);

        int damage = RollDamage(barrel, out bool isCrit);

        wave.Play(warhead.NearDiameter, warhead.FarDiameter, damage, Context.TargetFilter);

        if (isCrit)
            ShowCrit(centre, damage);
    }
}
