using UnityEngine;

public class WeaponContext
{
    public WeaponContext(Transform[] mounts, int barrels, PoolRegistry pools, ShotAudio audio,
        LayerMask targetLayers, Camera camera, float projectileSpeedScale = 1f, LayerMask worldLayers = default)
    {
        Mounts = mounts;
        Barrels = barrels;
        Pools = pools;
        Audio = audio;
        TargetLayers = targetLayers;
        Camera = camera;
        ProjectileSpeedScale = projectileSpeedScale;

        ContactFilter2D filter = new ContactFilter2D();

        filter.useTriggers = true;
        filter.SetLayerMask(targetLayers);

        TargetFilter = filter;

        ContactFilter2D world = new ContactFilter2D();

        world.useTriggers = true;
        world.SetLayerMask(worldLayers);

        WorldFilter = world;
    }

    public Transform[] Mounts { get; private set; }

    public int Barrels { get; private set; }

    public PoolRegistry Pools { get; private set; }

    public ShotAudio Audio { get; private set; }

    public LayerMask TargetLayers { get; private set; }

    public ContactFilter2D TargetFilter { get; private set; }

    public ContactFilter2D WorldFilter { get; private set; }

    public Camera Camera { get; private set; }

    public float ProjectileSpeedScale { get; private set; }
}
