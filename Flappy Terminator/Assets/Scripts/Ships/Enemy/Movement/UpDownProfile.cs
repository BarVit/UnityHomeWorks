using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Movement/Up And Down")]
public class UpDownProfile : MovementProfile
{
    [SerializeField, Range(0f, 1f)] private float _entryShare = 0.25f;
    [SerializeField] private float _duration = 30f;
    [SerializeField] private float _edgeMargin = 1f;
    [SerializeField, Range(0f, 1f)] private float _bodyGapShare;

    public float EntryShare => _entryShare;

    public float Duration => _duration;

    public float EdgeMargin => _edgeMargin;

    public float BodyGapShare => _bodyGapShare;

    public bool IsEndless => _duration <= 0f;

    public bool KeepsBodyGap => _bodyGapShare > 0f;

    public override EnemyMovement Create(Rigidbody2D body, Camera camera)
    {
        return new UpDownMovement(body, camera, this);
    }
}
