using UnityEngine;

public class OverheatSmoke : MonoBehaviour
{
    [SerializeField] private PlayerShooter _shooter;
    [SerializeField] private ParticleSystem _smoke;

    private bool _isSmoking;

    private void Update()
    {
        if (_shooter.IsOverheated == _isSmoking)
            return;

        _isSmoking = _shooter.IsOverheated;

        if (_isSmoking)
            _smoke.Play(true);
        else
            _smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
