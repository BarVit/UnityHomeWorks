using UnityEngine;

public class WeaponContext
{
    public WeaponContext(Transform[] mounts, int barrels, PoolRegistry pools, ShotAudio audio,
        ShotAudio hitAudio, LayerMask targetLayers, int ammoLayer, Camera camera, LayerMask worldLayers = default,
        ShotLoop loop = null, ShotLoop hitLoop = null, int damage = 0,
        WeaponBoost boost = default, CritMark critMark = null)
    {
        Mounts = mounts;
        Barrels = barrels;
        Pools = pools;
        Audio = audio;
        HitAudio = hitAudio;
        Loop = loop;
        HitLoop = hitLoop;
        TargetLayers = targetLayers;
        AmmoLayer = ammoLayer;
        Camera = camera;
        Damage = damage;
        Boost = boost;
        CritMark = critMark;

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

    public ShotAudio HitAudio { get; private set; }

    public ShotLoop Loop { get; private set; }

    public ShotLoop HitLoop { get; private set; }

    public LayerMask TargetLayers { get; private set; }

    public int AmmoLayer { get; private set; }

    public ContactFilter2D TargetFilter { get; private set; }

    public ContactFilter2D WorldFilter { get; private set; }

    public Camera Camera { get; private set; }

    public int Damage { get; private set; }

    public WeaponBoost Boost { get; private set; }

    public CritMark CritMark { get; private set; }
}
