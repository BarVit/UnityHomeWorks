using System;
using UnityEngine;
using System.Collections;

public class Stopwatch : MonoBehaviour
{
    [SerializeField] private float _delay = 0.5f;
    [SerializeField] private float _increaser = 1;

    private Coroutine _coroutine;

    public event Action TimeChanged;

    private bool _isCounting = false;

    public float Counter { get; private set; }

    private void Start()
    {
        _coroutine = StartCoroutine(Count());
    }

    private void Update()
    {
        int leftButton = 0;

        if (Input.GetMouseButton(leftButton))
            _isCounting = !_isCounting;
    }

    private IEnumerator Count()
    {
        WaitForSeconds delay = new WaitForSeconds(_delay);

        while (enabled)
        {
            if (_isCounting)
            {
                Counter += _increaser;
                TimeChanged?.Invoke();
            }

            yield return delay;
        }
    }
}