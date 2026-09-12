using System;
using UnityEngine;

public class InputReader : MonoBehaviour
{
    [SerializeField] private KeyCode _jumpKey = KeyCode.W;
    [SerializeField] private KeyCode _shootKey = KeyCode.Space;
    [SerializeField] private KeyCode _pauseKey = KeyCode.Escape;

    public event Action Jumped;
    public event Action Shot;
    public event Action PauseToggled;

    private void Update()
    {
        if (Input.GetKeyDown(_jumpKey))
            Jumped?.Invoke();

        if (Input.GetKeyDown(_shootKey))
            Shot?.Invoke();

        if (Input.GetKeyDown(_pauseKey))
            PauseToggled?.Invoke();
    }
}
