using UnityEngine;

public class OverheatSound : MonoBehaviour
{
    [SerializeField] private PlayerShooter _shooter;
    [SerializeField] private AudioSource _source;
    [SerializeField, Range(0f, 1f)] private float _volume = 0.8f;

    private bool _isOverheated;

    private void Update()
    {
        if (_shooter.IsOverheated == _isOverheated)
            return;

        _isOverheated = _shooter.IsOverheated;

        if (_isOverheated)
            Play();
    }

    private void Play()
    {
        if (_shooter.OverheatClip == null || _source.isActiveAndEnabled == false)
            return;

        _source.PlayOneShot(_shooter.OverheatClip, _volume);
    }
}
