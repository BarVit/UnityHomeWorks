using TMPro;
using UnityEngine;

public class HealthLabel : MonoBehaviour
{
    [SerializeField] private Health _health;
    [SerializeField] private TMP_Text _label;

    private void OnEnable()
    {
        _health.Changed += Show;
    }

    private void Start()
    {
        Show(_health.Value);
    }

    private void OnDisable()
    {
        _health.Changed -= Show;
    }

    private void Show(int value)
    {
        _label.text = $"{value} / {_health.MaxValue}";
    }
}
