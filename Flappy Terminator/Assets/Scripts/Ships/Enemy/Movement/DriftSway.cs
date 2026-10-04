using UnityEngine;

public class DriftSway
{
    private readonly DriftProfile _profile;

    private float _phase;

    public DriftSway(DriftProfile profile)
    {
        _profile = profile;
    }

    public void Reset()
    {
        _phase = Random.Range(0f, 2f * Mathf.PI);
    }

    public Vector2 Next(float deltaTime)
    {
        if (_profile.Period <= 0f)
            return Vector2.zero;

        _phase += deltaTime * 2f * Mathf.PI / _profile.Period;

        return Vector2.up * (Mathf.Cos(_phase) * _profile.Amplitude);
    }
}
