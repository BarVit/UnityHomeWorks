using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    [SerializeField] float _speed = 2.0f;

    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private List<Vector3> _waypoints;
    private Vector3 _firstPosition;
    private Vector3 _secondPositon;
    private Vector3 _waypoint;
    private float _secondPositionOffsetX = -4.5f;
    private float _stayAtWaypointTime = 2f;
    private int _index = 0;
    private int _direction = 0;
    private bool _isRunning = true;
    private string _animatorSpeed = "speed";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _firstPosition = new Vector3(transform.position.x, transform.position.y, 0);
        _secondPositon = new Vector3(transform.position.x + _secondPositionOffsetX, transform.position.y, 0);

        _waypoints = new()
        {
            _firstPosition,
            _secondPositon
        };

        _waypoint = _waypoints[_index];
    }

    private void Update()
    {
        SetFaceDirection();
        SetAnimation();
        Patrol();
    }

    private void SetFaceDirection()
    {
        if (_direction == 1)
            _spriteRenderer.flipX = true;
        else if (_direction == -1)
            _spriteRenderer.flipX = false;
    }

    private void SetAnimation()
    {
        if (_direction == 0)
            _animator.SetFloat(_animatorSpeed, 0);
        else
            _animator.SetFloat(_animatorSpeed, _speed);
    }

    private void Patrol()
    {
        if (transform.position.x >= _waypoint.x && _direction == 1)
            _direction = 0;
        else if (transform.position.x <= _waypoint.x && _direction == -1)
            _direction = 0;
        else if (transform.position.x < _waypoint.x && _isRunning)
            _direction = 1;
        else if (transform.position.x > _waypoint.x && _isRunning)
            _direction = -1;

        if (_direction == 0 && _isRunning)
        {
            _isRunning = false;
            StartCoroutine(StayAtWaypoint());
        }
        else
        {
            _rigidbody2D.velocity = new Vector2(_direction * _speed, _rigidbody2D.velocity.y);
        }
    }

    private IEnumerator StayAtWaypoint()
    {
        float randomMultiplayer = Random.Range(0.5f, 2f);
        float stayingTime = _stayAtWaypointTime * randomMultiplayer;

        while (stayingTime > 0)
        {
            stayingTime -= Time.deltaTime;

            yield return null;
        }

        _waypoint = GetNextWaypoint();
        _isRunning = true;
    }

    private Vector2 GetNextWaypoint()
    {
        _index = ++_index % _waypoints.Count;
        return _waypoints[_index];
    }
}