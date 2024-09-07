using TMPro;
using UnityEngine;
using System.Collections;

public class Counter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    private float _counter = 0;

    private bool _isCounting = false;

    private IEnumerator Count()
    {
        while (true)
        {
            if (_isCounting)
            {
                _counter += Time.deltaTime;
                DisplayCounter(_counter);
            }

            yield return null;
        }
    }

    private void DisplayCounter(float counter)
    {
        _text.text = counter.ToString();
    }

    private void Start()
    {
        StartCoroutine(Count());
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
            _isCounting = !_isCounting;
    }
}