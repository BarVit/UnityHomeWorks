using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VictoryScreen : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _score;
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _nextButton;

    public event Action MenuRequested;
    public event Action NextRequested;

    private void OnEnable()
    {
        _menuButton.onClick.AddListener(OnMenuClicked);
        _nextButton.onClick.AddListener(OnNextClicked);
    }

    private void OnDisable()
    {
        _menuButton.onClick.RemoveListener(OnMenuClicked);
        _nextButton.onClick.RemoveListener(OnNextClicked);
    }

    public void Show(int score, bool hasNextLocation)
    {
        _score.text = $"SCORE  {score}";
        _nextButton.gameObject.SetActive(hasNextLocation);
        _panel.SetActive(true);
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }

    private void OnMenuClicked()
    {
        MenuRequested?.Invoke();
    }

    private void OnNextClicked()
    {
        NextRequested?.Invoke();
    }
}
