using TMPro;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    private float _counter = 0;

    private bool _isCounting = false;

    private void Update()
    {
        if(Input.GetMouseButton(0))
        {
            _isCounting = !_isCounting;
        }

        if (_isCounting)
        {
            _counter += Time.deltaTime;
            _text.text = _counter.ToString();
        }
    }
}