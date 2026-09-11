using TMPro;
using UnityEngine;

public class ScoreViewer : MonoBehaviour
{
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private TMP_Text _score;

    private ObjectPoolEnemy _objectPoolEnemy;

    private void Start()
    {
        _objectPoolEnemy = _enemySpawner.GetPool();
        _objectPoolEnemy.KillsCountChanged += OnKillsChanged;
    }

    private void Destroy()
    {
        _objectPoolEnemy.KillsCountChanged -= OnKillsChanged;
    }

    private void OnKillsChanged(int killsCount)
    {
        _score.text = killsCount.ToString();
    }
}