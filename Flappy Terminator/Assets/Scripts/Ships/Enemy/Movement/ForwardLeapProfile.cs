using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Movement/Forward Leap")]
public class ForwardLeapProfile : MovementProfile
{
    [SerializeField] private float _leapSpeed = 5f;
    [SerializeField, Range(0f, 1f)] private float _minLeapStartShare = 0.1f;
    [SerializeField, Range(0f, 1f)] private float _maxLeapStartShare = 0.3f;
    [SerializeField, Range(0f, 1f)] private float _leapShare = 0.333f;
    [SerializeField] private float _afterLeapSpeed = 0.75f;

    public float LeapSpeed => _leapSpeed;

    public float LeapShare => _leapShare;

    public float AfterLeapSpeed => _afterLeapSpeed;

    public float GetLeapStartShare()
    {
        return Random.Range(_minLeapStartShare, _maxLeapStartShare);
    }

    public override EnemyMovement Create(Rigidbody2D body, Camera camera)
    {
        return new ForwardLeapMovement(body, camera, this);
    }
}
