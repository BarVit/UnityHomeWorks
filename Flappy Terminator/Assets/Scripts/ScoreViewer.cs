using TMPro;
using UnityEngine;

public class ScoreViewer : MonoBehaviour
{
    [SerializeField] private KillCounter _killCounter;
    [SerializeField] private TMP_Text _score;

    private void OnEnable()
    {
        _killCounter.Changed += OnKillsChanged;
    }

    private void OnDisable()
    {
        _killCounter.Changed -= OnKillsChanged;
    }

    private void OnKillsChanged(int killsCount)
    {
        _score.text = killsCount.ToString();
    }
}
