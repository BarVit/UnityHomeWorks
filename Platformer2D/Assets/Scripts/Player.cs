using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    [SerializeField] GroundSensor _groundSensor;
    [SerializeField] float _speed = 4.0f;
    [SerializeField] float _jumpForce = 7.5f;

    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private string _animatorSpeed = "speed";
    private string _animatorGrounded = "grounded";
    private string _animatorJump = "jump";
    private string _animatorAttack = "attack";
    private string _buttonSpace = "space";
    private string _horizontal = "Horizontal";
    private int _coinsCount = 0;
    private bool _grounded;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        float inputX = Input.GetAxis(_horizontal);

        CheckGroundState();
        SetFaceDirection(inputX);
        HandleInput(inputX);
        Move(inputX);
    }

    public void TakeCoin()
    {
        _coinsCount++;
    }

    private void CheckGroundState()
    {
        if (_groundSensor.IsGrounded && _grounded == false)
        {
            _grounded = true;
            _animator.SetBool(_animatorGrounded, _grounded);
        }

        if (_groundSensor.IsGrounded == false && _grounded)
        {
            _grounded = false;
            _animator.SetBool(_animatorGrounded, _grounded);
        }
    }

    private void SetFaceDirection(float inputX)
    {
        if (inputX > 0)
            _spriteRenderer.flipX = false;
        else if (inputX < 0)
            _spriteRenderer.flipX = true;
    }

    private void HandleInput(float inputX)
    {
        if (Input.GetMouseButtonDown(0))
        {
            _animator.SetTrigger(_animatorAttack);
        }
        else if (Input.GetKeyDown(_buttonSpace) && _grounded)
        {
            _animator.SetTrigger(_animatorJump);
            _grounded = false;
            _animator.SetBool(_animatorGrounded, _grounded);
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpForce);
        }
        else if (Mathf.Abs(inputX) > 0)
        {
            _animator.SetFloat(_animatorSpeed, _speed);
        }
        else if (inputX == 0)
        {
            _animator.SetFloat(_animatorSpeed, 0);
        }
    }

    private void Move(float inputX)
    {
        _rigidbody2D.velocity = new Vector2(inputX * _speed, _rigidbody2D.velocity.y);
    }
}