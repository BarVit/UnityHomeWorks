using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _score;
    [SerializeField] private Button _restartButton;

    public event Action RestartRequested;

    private void OnEnable()
    {
        _restartButton.onClick.AddListener(OnRestartClicked);
    }

    private void OnDisable()
    {
        _restartButton.onClick.RemoveListener(OnRestartClicked);
    }

    public void Show(int score)
    {
        _score.text = score.ToString();
        _panel.SetActive(true);
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }

    private void OnRestartClicked()
    {
        RestartRequested?.Invoke();
    }
}
