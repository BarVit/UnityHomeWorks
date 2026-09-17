using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    [SerializeField] private Key _jumpKey = Key.W;
    [SerializeField] private Key _shootKey = Key.Space;
    [SerializeField] private Key _pauseKey = Key.Escape;

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
}
