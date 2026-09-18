using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    [SerializeField] private Key _jumpKey = Key.W;
    [SerializeField] private Key _shootKey = Key.Space;
    [SerializeField] private Key _pauseKey = Key.Escape;

    private bool _isFireHeld;

    public bool IsShooting => _isFireHeld || IsKeyHeld(_shootKey);

    public event Action Jumped;
    public event Action Shot;
    public event Action Descended;
    public event Action PauseToggled;

    private void Update()
    {
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

    private bool IsKeyHeld(Key key)
    {
        Keyboard keyboard = Keyboard.current;

        return keyboard != null && keyboard[key].isPressed;
    }
}
