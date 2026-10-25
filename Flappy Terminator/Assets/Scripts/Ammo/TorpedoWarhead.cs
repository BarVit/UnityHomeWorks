using System;
using UnityEngine;

public class TorpedoWarhead : MonoBehaviour
{
    [SerializeField] private BlastWave _blast;
    [SerializeField] private Sound _blastSound;
    [SerializeField] private float _nearRadius = 0.3f;
    [SerializeField] private float _farRadius = 1.5f;
    [SerializeField] private float _blastNoseShare = 0.25f;
    [SerializeField] private float _fuseNoseShare = 0.1f;
    [SerializeField] private float _fuseDelay = 0.3f;

    private readonly Collider2D[] _near = new Collider2D[8];
    private readonly RaycastHit2D[] _ahead = new RaycastHit2D[8];

    private Ammo _ammo;
    private Camera _camera;
    private ContactFilter2D _targets;
    private Action<Ammo, TorpedoWarhead> _detonate;
    private float _area;
    private float _armedFuseDelay;
    private float _fuseLeft;

    public BlastWave Blast => _blast;

    public Sound BlastSound => _blastSound;

    public float Length => GetSize().x;

    public float Width => GetSize().y;

    public float Radius => _farRadius;

    public float NearDiameter => _nearRadius * 2f;

    public float FarDiameter => _farRadius * 2f;

    public float GetFarDiameter(float radius)
    {
        return radius > 0f ? radius * 2f : FarDiameter;
    }

    public Vector2 Centre => GetPoint(_blastNoseShare);

    public Vector2 Sensor => GetPoint(_fuseNoseShare);

    public bool IsFuseBurning => _fuseLeft > 0f;

    private void Awake()
    {
        _ammo = GetComponent<Ammo>();
    }

    private void Update()
    {
        if (_detonate == null || _ammo == null || _ammo.IsArmed == false)
            return;

        if (IsFuseBurning)
        {
            _fuseLeft -= Time.deltaTime;

            if (_fuseLeft <= 0f)
                Detonate();

            return;
        }

        if (ScreenArea.Contains(_camera, Sensor) == false || IsOnCourse())
            return;

        int count = Physics2D.OverlapCircle(Sensor, _area, _targets, _near);

        for (int i = 0; i < count; i++)
        {
            if (WeaponDelivery.IsOpenTarget(_near[i]))
            {
                _fuseLeft = _armedFuseDelay;

                return;
            }
        }
    }

    public void Arm(float area, float fuseDelay, ContactFilter2D targets, Camera camera,
        Action<Ammo, TorpedoWarhead> detonate)
    {
        _area = area;
        _armedFuseDelay = fuseDelay > 0f ? fuseDelay : _fuseDelay;
        _targets = targets;
        _camera = camera;
        _detonate = detonate;
        _fuseLeft = 0f;
    }

    private void Detonate()
    {
        _fuseLeft = 0f;

        Action<Ammo, TorpedoWarhead> detonate = _detonate;

        _detonate = null;
        detonate?.Invoke(_ammo, this);
    }

    private bool IsOnCourse()
    {
        Vector2 course = transform.right;
        Vector2 edge = new Vector2(-course.y, course.x) * (Width * 0.5f);

        return SeesTarget(Sensor + edge, course) || SeesTarget(Sensor - edge, course);
    }

    private bool SeesTarget(Vector2 origin, Vector2 course)
    {
        int count = Physics2D.Raycast(origin, course, _targets, _ahead, _area);

        for (int i = 0; i < count; i++)
        {
            if (WeaponDelivery.IsOpenTarget(_ahead[i].collider))
                return true;
        }

        return false;
    }

    private Vector2 GetPoint(float noseShare)
    {
        return (Vector2)transform.position
            + (Vector2)transform.right * (Length * (0.5f - noseShare));
    }

    private Vector2 GetSize()
    {
        SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();

        return renderer == null ? Vector2.one : (Vector2)renderer.bounds.size;
    }
}
