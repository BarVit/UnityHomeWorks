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
    public event Action PauseToggled;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard[_jumpKey].wasPressedThisFrame)
            Jumped?.Invoke();

        if (keyboard[_shootKey].wasPressedThisFrame)
            Shot?.Invoke();

        if (keyboard[_pauseKey].wasPressedThisFrame)
            PauseToggled?.Invoke();
    }
}
