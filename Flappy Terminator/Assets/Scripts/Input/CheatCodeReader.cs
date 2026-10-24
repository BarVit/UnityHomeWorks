using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CheatCodeReader : MonoBehaviour
{
    [SerializeField] private Key[] _code = { Key.O, Key.P };
    [SerializeField] private float _maxInterval = 0.3f;

    private int _typed;
    private float _lastPressTime;

    public event Action Entered;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        foreach (KeyControl key in keyboard.allKeys)
        {
            if (key != null && key.wasPressedThisFrame)
                Read(key.keyCode);
        }
    }

    private void Read(Key key)
    {
        if (IsTooLate())
            _typed = 0;

        if (key == _code[_typed])
            _typed++;
        else
            _typed = key == _code[0] ? 1 : 0;

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
