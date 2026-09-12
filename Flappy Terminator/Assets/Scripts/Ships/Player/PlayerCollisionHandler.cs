using System;
using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    private const float AsteroidBumpGravityFromAbove = -2f;
    private const float AsteroidBumpGravityFromBelow = 4f;
    private const float EnemyBumpGravity = 1f;

    [SerializeField] private BumpEffect _bumpPrefab;

    private SpawnPool<BumpEffect> _bumpPool;

    public event Action<Asteroid> AsteroidBumped;
    public event Action<EnemyShip> EnemyBumped;

    private void Awake()
    {
        _bumpPool = new SpawnPool<BumpEffect>(_bumpPrefab);
    }

    private void OnValidate()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void ReleaseAll()
    {
        _bumpPool.ReleaseAll();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Asteroid asteroid))
        {
            AsteroidBumped?.Invoke(asteroid);

            PlayBump(other, transform.position.y > 0
                ? AsteroidBumpGravityFromAbove
                : AsteroidBumpGravityFromBelow);
        }

        if (other.TryGetComponent(out EnemyCollision enemyCollision))
        {
            EnemyBumped?.Invoke(enemyCollision.GetShip());
            PlayBump(other, EnemyBumpGravity);
            enemyCollision.Bump();
        }
    }

    private void PlayBump(Collider2D other, float gravityModifier)
    {
        _bumpPool.Get(other.ClosestPoint(transform.position)).Play(gravityModifier);
    }
}
