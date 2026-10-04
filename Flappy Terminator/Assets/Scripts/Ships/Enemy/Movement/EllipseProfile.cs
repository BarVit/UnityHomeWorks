using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Movement/Ellipse")]
public class EllipseProfile : MovementProfile
{
    [SerializeField, Range(0f, 1f)] private float _minRadiusShare = 0.3f;
    [SerializeField, Range(0f, 1f)] private float _maxRadiusShare = 0.6f;
    [SerializeField, Range(0f, 1f)] private float _backwardSpeedShare = 0.5f;
    [SerializeField, Range(0f, 1f)] private float _loopStartShare = 0.1f;
    [SerializeField] private float _edgeMargin = 1f;

    public float BackwardSpeedShare => _backwardSpeedShare;

    public float LoopStartShare => _loopStartShare;

    public float GetRadiusShare()
    {
        return Random.Range(_minRadiusShare, _maxRadiusShare);
    }

    public override Vector2 GetSpawnPoint(Camera camera, float margin, float minY, float maxY)
    {
        float centerY = camera.transform.position.y;
        float edge = camera.orthographicSize - _edgeMargin;

        return new Vector2(ScreenArea.GetRightEdge(camera) + margin,
            Random.value < 0.5f ? centerY + edge : centerY - edge);
    }

    public override EnemyMovement Create(Rigidbody2D body, Camera camera)
    {
        return new EllipseMovement(body, camera, this);
    }
}
