using UnityEngine;

[RequireComponent(typeof(PlayerMover), typeof(PlayerShooter))]
public class PlayerInputRouter : MonoBehaviour
{
    [SerializeField] private InputReader _input;

    private PlayerMover _mover;
    private PlayerShooter _shooter;
    private bool _isEnabled = true;

    public bool IsEnabled => _isEnabled;

    private void Awake()
    {
        _mover = GetComponent<PlayerMover>();
        _shooter = GetComponent<PlayerShooter>();
    }

    private void OnEnable()
    {
        _input.Jumped += OnJumped;
    }

    private void OnDisable()
    {
        _input.Jumped -= OnJumped;
    }

    public void Unlock()
    {
        _isEnabled = true;
    }

    public void Lock()
    {
        _isEnabled = false;
    }

    private void OnJumped()
    {
        if (_isEnabled == false)
            return;

        _mover.Jump();
    }

    private void Update()
    {
        if (_isEnabled == false)
            return;

        if (_input.IsShooting)
            _shooter.Shoot();
    }
}
