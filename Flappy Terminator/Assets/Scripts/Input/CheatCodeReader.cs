using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheatCodeReader : MonoBehaviour
{
    [SerializeField] private Key[] _code = { Key.O, Key.P };
    [SerializeField] private float _maxInterval = 0.2f;

    private int _typed;
    private float _lastPressTime;

    public event Action Entered;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null || keyboard.anyKey.wasPressedThisFrame == false)
            return;

        if (IsTooLate())
            _typed = 0;

        if (keyboard[_code[_typed]].wasPressedThisFrame)
            _typed++;
        else
            _typed = keyboard[_code[0]].wasPressedThisFrame ? 1 : 0;

        _lastPressTime = Time.unscaledTime;

        if (_typed < _code.Length)
            return;

        _typed = 0;
        Entered?.Invoke();
    }

    private bool IsTooLate()
    {
        return _typed > 0 && Time.unscaledTime - _lastPressTime > _maxInterval;
    }
}
