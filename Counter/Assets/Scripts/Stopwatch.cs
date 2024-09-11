using System;
using UnityEngine;
using System.Collections;

public class Stopwatch : MonoBehaviour
{
    [SerializeField] private float _delay = 0.5f;
    [SerializeField] private float _step = 1;

    public event Action TimeChanged;

    private Coroutine _coroutine;

    private int _leftButton = 0;

    private bool _isCounting = false;

    public float Counter { get; private set; }

    private void Update()
    {
        if (Input.GetMouseButton(_leftButton))
        {
            _isCounting = !_isCounting;
            HandleCoroutine();
        }
    }

    private void HandleCoroutine()
    {
        if (_isCounting)
            _coroutine = StartCoroutine(Count());
        else
            if (_coroutine != null)
                StopCoroutine(_coroutine);
    }

    private IEnumerator Count()
    {
        WaitForSeconds delay = new WaitForSeconds(_delay);

        while (enabled)
        {
            Counter += _step;
            TimeChanged?.Invoke();
            yield return delay;
        }
    }
}