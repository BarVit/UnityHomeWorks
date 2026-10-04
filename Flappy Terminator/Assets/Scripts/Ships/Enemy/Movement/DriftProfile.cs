using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Movement/Drift")]
public class DriftProfile : MovementProfile
{
    [SerializeField] private float _amplitude = 1.5f;
    [SerializeField] private float _period = 2f;

    public float Amplitude => _amplitude;

    public float Period => _period;

    public override EnemyMovement Create(Rigidbody2D body, Camera camera)
    {
        return new DriftMovement(body, camera, new DriftSway(this));
    }
}
