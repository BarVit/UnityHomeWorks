using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AimLine : MonoBehaviour
{
    [SerializeField] private float _restDuration = 1f;

    private LineRenderer _line;
    private PlayerShip _ship;
    private Camera _camera;
    private Transform _mount;
    private Quaternion _offset = Quaternion.identity;
    private float _restLeft;

    public Vector3 From { get; private set; }

    public Quaternion Rotation { get; private set; }

    public float AimedTime { get; private set; }

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _ship = GetComponentInParent<PlayerShip>();
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_mount == null)
            return;

        if (_restLeft > 0f)
            _restLeft -= Time.deltaTime;

        From = _mount.position;
        Rotation = _mount.rotation * _offset;

        Vector3 direction = Rotation * Vector3.right;

        _line.SetPosition(0, From);
        _line.SetPosition(1, From + direction * GetLength(direction));
        _line.enabled = _restLeft <= 0f && (_ship == null || _ship.IsControlEnabled);
        AimedTime = _line.enabled ? AimedTime + Time.deltaTime : 0f;
    }

    public void Follow(Transform mount, Quaternion offset)
    {
        _mount = mount;
        _offset = offset;
        From = mount.position;
        Rotation = mount.rotation * offset;
    }

    public void Rest()
    {
        _restLeft = _restDuration;
        _line.enabled = false;
        AimedTime = 0f;
    }

    private float GetLength(Vector3 direction)
    {
        float width = ScreenArea.GetRightEdge(_camera) - ScreenArea.GetLeftEdge(_camera);

        if (direction.x > 0.01f)
            return Mathf.Min((ScreenArea.GetRightEdge(_camera) - From.x) / direction.x, width * 2f);

        if (direction.x < -0.01f)
            return Mathf.Min((From.x - ScreenArea.GetLeftEdge(_camera)) / -direction.x, width * 2f);

        return width;
    }
}
