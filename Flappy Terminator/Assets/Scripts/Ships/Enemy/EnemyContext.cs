using UnityEngine;

public class EnemyContext
{
    public EnemyContext(PoolRegistry pools, ExplosionAnimation explosionPrefab,
        EnemyDeadBody deadBodyPrefab, ShotAudio hitAudio, LayerMask targetLayers, Camera camera)
    {
        Pools = pools;
        Explosions = pools.GetPool(explosionPrefab);
        DeadBodies = pools.GetPool(deadBodyPrefab);
        HitAudio = hitAudio;
        TargetLayers = targetLayers;
        Camera = camera;
    }

    public PoolRegistry Pools { get; private set; }

    public SpawnPool<ExplosionAnimation> Explosions { get; private set; }

    public SpawnPool<EnemyDeadBody> DeadBodies { get; private set; }

    public ShotAudio HitAudio { get; private set; }

    public LayerMask TargetLayers { get; private set; }

    public Camera Camera { get; private set; }
}
