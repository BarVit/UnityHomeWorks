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

        Transform target = _shooter.LockTarget;
        bool isOnTarget = _input.IsShooting && IsAlive(target);

        if (isOnTarget && _wasOnTarget == false && _mover.IsHolding == false)
            _mover.Hold(GetDuration(), target);

        _wasOnTarget = isOnTarget;
    }

    private float GetDuration()
    {
        return _shooter.TargetHoldTime > 0f ? _shooter.TargetHoldTime : _duration;
    }

    private bool IsAlive(Transform target)
    {
        return target != null && target.gameObject.activeInHierarchy;
    }
}
