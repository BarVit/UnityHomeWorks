using System;
using UnityEngine;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _startButton;

    public event Action StartRequested;

    private void OnEnable()
    {
        _startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnDisable()
    {
        _startButton.onClick.RemoveListener(OnStartClicked);
    }

    public void Show()
    {
        _panel.SetActive(true);
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }

    private void OnStartClicked()
    {
        StartRequested?.Invoke();
    }
}
