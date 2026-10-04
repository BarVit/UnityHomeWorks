using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Movement/Up And Down")]
public class UpDownProfile : MovementProfile
{
    [SerializeField, Range(0f, 1f)] private float _entryShare = 0.25f;
    [SerializeField] private float _duration = 30f;
    [SerializeField] private float _edgeMargin = 1f;

    public float EntryShare => _entryShare;

    public float Duration => _duration;

    public float EdgeMargin => _edgeMargin;

    public override EnemyMovement Create(Rigidbody2D body, Camera camera)
    {
        return new UpDownMovement(body, camera, this);
    }
}
