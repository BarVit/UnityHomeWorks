using UnityEngine;

public class TorpedoDelivery : ProjectileDelivery
{
    private SpawnPool<BlastWave> _blastPool;

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
            warhead.Arm(Row.Area, Row.Blast.FuseDelay, Context.TargetFilter, Context.Camera, OnDetonated);

        return torpedo;
    }

    private void OnDetonated(Ammo torpedo, TorpedoWarhead warhead)
    {
        if (torpedo.TryScore())
            CountHit();

        Blast(warhead, torpedo.Barrel);
        torpedo.Remove();
    }

    private void OnStruck(Ammo torpedo)
    {
        if (torpedo.TryGetComponent(out TorpedoWarhead warhead))
            Blast(warhead, torpedo.Barrel);
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
        float far = warhead.GetFarDiameter(Row.Blast.Radius);

        wave.Play(Mathf.Min(warhead.NearDiameter, far), far, damage, Context.TargetFilter, Row.Blast.Time);

        if (isCrit)
            ShowCrit(centre, damage);
    }
}
