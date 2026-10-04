using UnityEngine;

public class ShipExplosion : MonoBehaviour
{
    private const int PoolCapacity = 1;
    private const int PoolMaxSize = 1;
    private const string Section = "Ship Explosions";

    [SerializeField] private ExplosionAnimation _prefab;

    private SpawnPool<ExplosionAnimation> _pool;

    private void Awake()
    {
        _pool = new SpawnPool<ExplosionAnimation>(_prefab, SpawnFolder.Get(Section), PoolCapacity, PoolMaxSize);
    }

    public void Play(Vector3 position)
    {
        ExplosionAnimation explosion = _pool.Get(position);

        explosion.Play();
    }

    public void ReleaseAll()
    {
        _pool.ReleaseAll();
    }
}
