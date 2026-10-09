using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VictoryScreen : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _score;
    [SerializeField] private Button _menuButton;

    public event Action MenuRequested;

    private void OnEnable()
    {
        _menuButton.onClick.AddListener(OnMenuClicked);
    }

    private void OnDisable()
    {
        _menuButton.onClick.RemoveListener(OnMenuClicked);
    }

    public void Show(int score)
    {
        _score.text = $"SCORE  {score}";
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
}
