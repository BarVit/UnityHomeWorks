using UnityEngine;

public class TorpedoWarhead : MonoBehaviour
{
    [SerializeField] private BlastWave _blast;
    [SerializeField] private float _nearShare = 0.5f;
    [SerializeField] private float _farShare = 2f;

    public BlastWave Blast => _blast;

    public float Length => GetLength();

    public float NearDiameter => GetLength() * _nearShare;

    public float FarDiameter => GetLength() * _farShare;

    private float GetLength()
    {
        SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();

        return renderer == null ? 1f : renderer.bounds.size.x;
    }
}
