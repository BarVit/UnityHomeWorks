using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Movement/Drift Retreat")]
public class DriftRetreatProfile : DriftProfile
{
    [SerializeField, Range(0f, 1f)] private float _advanceShare = 0.667f;
    [SerializeField, Range(0f, 1f)] private float _retreatShare = 0.5f;

    public float AdvanceShare => _advanceShare;

    public float RetreatShare => _retreatShare;

    public override EnemyMovement Create(Rigidbody2D body, Camera camera)
    {
        return new DriftRetreatMovement(body, camera, this, new DriftSway(this));
    }
}
