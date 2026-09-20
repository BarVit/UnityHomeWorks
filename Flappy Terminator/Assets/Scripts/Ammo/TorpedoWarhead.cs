using UnityEngine;

public class TorpedoWarhead : MonoBehaviour
{
    [SerializeField] private BlastWave _blast;
    [SerializeField] private float _nearShare = 0.5f;
    [SerializeField] private float _farShare = 2f;
    [SerializeField] private float _fuseDelay = 0.3f;

    private float _fuseLeft;

    public BlastWave Blast => _blast;

    public float Length => GetSize().x;

    public float Width => GetSize().y;

    public float NearDiameter => Length * _nearShare;

    public float FarDiameter => Length * _farShare;

    public bool IsFuseBurning => _fuseLeft > 0f;

    public void StartFuse()
    {
        _fuseLeft = _fuseDelay;
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

    private Vector2 GetSize()
    {
        SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();

        return renderer == null ? Vector2.one : (Vector2)renderer.bounds.size;
    }
}
