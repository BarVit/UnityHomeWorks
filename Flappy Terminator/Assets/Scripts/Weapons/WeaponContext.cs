using UnityEngine;

public class WeaponContext
{
    public WeaponContext(Transform[] mounts, int barrels, PoolRegistry pools, ShotAudio audio,
        LayerMask targetLayers, Camera camera)
    {
        Mounts = mounts;
        Barrels = barrels;
        Pools = pools;
        Audio = audio;
        TargetLayers = targetLayers;
        Camera = camera;

        ContactFilter2D filter = new ContactFilter2D();

        filter.useTriggers = true;
        filter.SetLayerMask(targetLayers);

        TargetFilter = filter;
    }

    public Transform[] Mounts { get; private set; }

    public int Barrels { get; private set; }

    public PoolRegistry Pools { get; private set; }

    public ShotAudio Audio { get; private set; }

    public LayerMask TargetLayers { get; private set; }

    public ContactFilter2D TargetFilter { get; private set; }

    public Camera Camera { get; private set; }
}
