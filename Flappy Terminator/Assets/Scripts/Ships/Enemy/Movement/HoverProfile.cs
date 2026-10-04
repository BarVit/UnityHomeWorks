using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Movement/Hover")]
public class HoverProfile : MovementProfile
{
    [SerializeField, Range(0f, 1f)] private float _minFirstStopShare = 0.1f;
    [SerializeField, Range(0f, 1f)] private float _maxFirstStopShare = 0.1f;
    [SerializeField, Range(0f, 1f)] private float _stopStep = 0.33f;
    [SerializeField, Min(1)] private int _stopCount = 3;
    [SerializeField, Range(0f, 1f)] private float _stopChance = 0.5f;
    [SerializeField] private float _minHold = 3f;
    [SerializeField] private float _maxHold = 5f;

    public float StopStep => _stopStep;

    public int StopCount => _stopCount;

    public float StopChance => _stopChance;

    public float GetFirstStopShare()
    {
        return Random.Range(_minFirstStopShare, _maxFirstStopShare);
    }

    public float GetHold()
    {
        return Random.Range(_minHold, _maxHold);
    }

    public override EnemyMovement Create(Rigidbody2D body, Camera camera)
    {
        return new HoverMovement(body, camera, this);
    }
}
