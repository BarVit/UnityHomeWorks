using TMPro;
using UnityEngine;

public class KillsViewer : MonoBehaviour
{
    [SerializeField] private BattleStats _stats;
    [SerializeField] private TMP_Text _label;

    private void OnEnable()
    {
        _stats.Changed += Show;
    }

    private void Start()
    {
        Show();
    }

    private void OnDisable()
    {
        _stats.Changed -= Show;
    }

    private void Show()
    {
        _label.text = _stats.Killed + " / " + _stats.TotalEnemies;
    }
}
