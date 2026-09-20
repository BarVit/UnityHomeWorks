using UnityEngine;

[RequireComponent(typeof(PlayerMover), typeof(PlayerShooter))]
public class BeamAimLock : MonoBehaviour
{
    [SerializeField] private InputReader _input;
    [SerializeField] private float _duration = 0.5f;

    private PlayerMover _mover;
    private PlayerShooter _shooter;
    private bool _wasOnTarget;

    private void Awake()
    {
        _mover = GetComponent<PlayerMover>();
        _shooter = GetComponent<PlayerShooter>();
    }

    private void OnDisable()
    {
        _wasOnTarget = false;
    }

    private void Update()
    {
        if (_mover.enabled == false)
        {
            _wasOnTarget = false;

            return;
        }

        bool isOnTarget = _input.IsShooting && _shooter.LockTarget != null;

        if (isOnTarget && _wasOnTarget == false && _mover.IsHolding == false)
            _mover.Hold(_duration, _shooter.LockTarget);

        _wasOnTarget = isOnTarget;
    }
}
