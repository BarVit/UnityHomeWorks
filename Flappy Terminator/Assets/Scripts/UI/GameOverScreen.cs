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

    public void Show(Reward reward)
    {
        _score.text = $"+{NumberFormat.Compact(reward.Xp)} XP   +{NumberFormat.Compact(reward.Money)} COINS";
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
