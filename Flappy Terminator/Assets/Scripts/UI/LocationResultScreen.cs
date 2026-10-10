using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocationResultScreen : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Image[] _stars;
    [SerializeField] private Color _starOn = new Color(1f, 0.82f, 0.3f);
    [SerializeField] private Color _starOff = new Color(0.25f, 0.32f, 0.42f);
    [SerializeField] private TMP_Text _enemies;
    [SerializeField] private TMP_Text _accuracy;
    [SerializeField] private TMP_Text _damage;
    [SerializeField] private TMP_Text _earned;
    [SerializeField] private GameObject _noDamageRow;
    [SerializeField] private TMP_Text _noDamage;
    [SerializeField] private GameObject _noMissRow;
    [SerializeField] private TMP_Text _noMiss;
    [SerializeField] private Button _dockButton;

    public event Action DockRequested;

    private void OnEnable()
    {
        _dockButton.onClick.AddListener(OnDockClicked);
    }

    private void OnDisable()
    {
        _dockButton.onClick.RemoveListener(OnDockClicked);
    }

    public void Show(BattleStats stats, Reward earned)
    {
        for (int i = 0; i < _stars.Length; i++)
            _stars[i].color = i < stats.Stars ? _starOn : _starOff;

        _enemies.text = stats.Killed + " / " + stats.TotalEnemies;
        _accuracy.text = stats.Accuracy + "%";
        _damage.text = stats.DamageTaken.ToString();
        _earned.text = Format(earned);
        ShowBonus(_noDamageRow, _noDamage, stats.NoDamageBonus);
        ShowBonus(_noMissRow, _noMiss, stats.NoMissBonus);
        _panel.SetActive(true);
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }

    private static void ShowBonus(GameObject row, TMP_Text value, Reward bonus)
    {
        row.SetActive(bonus.IsEmpty == false);
        value.text = Format(bonus);
    }

    private static string Format(Reward reward)
    {
        return "+" + NumberFormat.Compact(reward.Xp) + " XP   +" + NumberFormat.Compact(reward.Money) + " COINS";
    }

    private void OnDockClicked()
    {
        DockRequested?.Invoke();
    }
}
