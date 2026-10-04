using UnityEngine;

public abstract class MovementProfile : ScriptableObject
{
    [SerializeField] private float _minSpeed = 0.5f;
    [SerializeField] private float _maxSpeed = 3f;

    public float GetSpeed()
    {
        return Random.Range(_minSpeed, _maxSpeed);
    }

    public virtual Vector2 GetSpawnPoint(Camera camera, float margin, float minY, float maxY)
    {
        return new Vector2(ScreenArea.GetRightEdge(camera) + margin, Random.Range(minY, maxY));
    }

    public abstract EnemyMovement Create(Rigidbody2D body, Camera camera);
}
