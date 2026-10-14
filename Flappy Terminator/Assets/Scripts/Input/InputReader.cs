using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    [SerializeField] private Key _jumpKey = Key.W;
    [SerializeField] private Key _shootKey = Key.Space;
    [SerializeField] private Key _pauseKey = Key.Escape;
    [SerializeField] private Key[] _moduleKeys = { Key.Digit1, Key.Digit2, Key.Digit3 };

    private bool _isFireHeld;
    private bool _isFireConsumed;

    public bool IsShooting => IsFireDown && _isFireConsumed == false;

    public event Action Jumped;
    public event Action Shot;
    public event Action Tapped;
    public event Action Descended;
    public event Action PauseToggled;
    public event Action<int> ModuleActivated;

    private void Update()
    {
        if (_isFireConsumed && IsFireDown == false)
            _isFireConsumed = false;

        if (WasScreenTapped())
            Tapped?.Invoke();

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard[_jumpKey].wasPressedThisFrame)
            Jump();

        if (keyboard[_shootKey].wasPressedThisFrame)
            Shoot();

        if (keyboard[_pauseKey].wasPressedThisFrame)
            TogglePause();

        for (int i = 0; i < _moduleKeys.Length; i++)
        {
            if (keyboard[_moduleKeys[i]].wasPressedThisFrame)
                ActivateModule(i);
        }
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

    public void ActivateModule(int slot)
    {
        ModuleActivated?.Invoke(slot);
    }

    private bool IsFireDown => _isFireHeld || IsKeyHeld(_shootKey);

    private bool WasScreenTapped()
    {
        Touchscreen touchscreen = Touchscreen.current;
        Mouse mouse = Mouse.current;

        return (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            || (mouse != null && mouse.leftButton.wasPressedThisFrame);
    }

    private bool IsKeyHeld(Key key)
    {
        Keyboard keyboard = Keyboard.current;

        return keyboard != null && keyboard[key].isPressed;
    }
}
