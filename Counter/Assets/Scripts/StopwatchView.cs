using TMPro;
using UnityEngine;

public class StopwatchView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _UserInterfaceText;

    [SerializeField] private Stopwatch _stopwatch;

    private void OnEnable()
    {
        _stopwatch.TimeChanged += Display;
    }

    private void OnDisable()
    {
        _stopwatch.TimeChanged -= Display;
    }

    private void Display()
    {
        _UserInterfaceText.text = _stopwatch.Counter.ToString();
    }
}