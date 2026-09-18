using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    [SerializeField] private Key _jumpKey = Key.W;
    [SerializeField] private Key _shootKey = Key.Space;
    [SerializeField] private Key _pauseKey = Key.Escape;

    private bool _isFireHeld;
    private bool _isFireConsumed;

    public bool IsShooting => IsFireDown && _isFireConsumed == false;

    public event Action Jumped;
    public event Action Shot;
    public event Action Descended;
    public event Action PauseToggled;

    private void Update()
    {
        if (_isFireConsumed && IsFireDown == false)
            _isFireConsumed = false;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard[_jumpKey].wasPressedThisFrame)
            Jump();

        if (keyboard[_shootKey].wasPressedThisFrame)
            Shoot();

        if (keyboard[_pauseKey].wasPressedThisFrame)
            TogglePause();
    }

    public void ConsumeFire()
    {
        _isFireConsumed = true;
    }

    public void HoldFire()
    {
        _isFireHeld = true;
    }

    public void ReleaseFire()
    {
        _isFireHeld = false;
    }

    public void Jump()
    {
        Jumped?.Invoke();
    }

    public void Descend()
    {
        Descended?.Invoke();
    }

    public void Shoot()
    {
        Shot?.Invoke();
    }

    public void TogglePause()
    {
        PauseToggled?.Invoke();
    }

    private bool IsFireDown => _isFireHeld || IsKeyHeld(_shootKey);

    private bool IsKeyHeld(Key key)
    {
        Keyboard keyboard = Keyboard.current;

        return keyboard != null && keyboard[key].isPressed;
    }
}
