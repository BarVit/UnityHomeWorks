using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMover : MonoBehaviour
{
    private readonly Dictionary<MovementProfile, EnemyMovement> _movements =
        new Dictionary<MovementProfile, EnemyMovement>();

    private Rigidbody2D _rigidbody2D;
    private Camera _camera;
    private EnemyMovement _movement;
    private Vector2 _frozenVelocity;
    private bool _isFrozen;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_movement == null || _isFrozen)
            return;

        _movement.Tick(Time.deltaTime);
    }

    public void Fly(MovementProfile profile)
    {
        _movement = GetMovement(profile);
        _movement.Begin(profile.GetSpeed());
    }

    public void Freeze()
    {
        if (_isFrozen)
            return;

        _isFrozen = true;
        _frozenVelocity = _rigidbody2D.linearVelocity;
        _rigidbody2D.linearVelocity = Vector2.zero;
    }

    public void Unfreeze()
    {
        if (_isFrozen == false)
            return;

        _isFrozen = false;
        _rigidbody2D.linearVelocity = _frozenVelocity;
    }

    public void Stop()
    {
        _isFrozen = false;

        if (_movement == null)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;

            return;
        }

        _movement.Stop();
        _movement = null;
    }

    private EnemyMovement GetMovement(MovementProfile profile)
    {
        if (_movements.TryGetValue(profile, out EnemyMovement movement))
            return movement;

        movement = profile.Create(_rigidbody2D, _camera);
        _movements.Add(profile, movement);

        return movement;
    }
}
