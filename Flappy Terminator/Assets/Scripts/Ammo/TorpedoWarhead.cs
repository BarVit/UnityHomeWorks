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

    public void StartFuse(float delay)
    {
        _fuseLeft = delay > 0f ? delay : _fuseDelay;
    }

    public void Cancel()
    {
        _fuseLeft = 0f;
    }

    public bool Burn(float deltaTime)
    {
        if (_fuseLeft <= 0f)
            return false;

        _fuseLeft -= deltaTime;

        return _fuseLeft <= 0f;
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
