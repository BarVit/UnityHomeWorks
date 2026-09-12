using TMPro;
using UnityEngine;

public class LevelViewer : MonoBehaviour
{
    [SerializeField] private Game _game;
    [SerializeField] private TMP_Text _label;

    private void OnEnable()
    {
        _game.LevelChanged += Show;
    }

    private void OnDisable()
    {
        _game.LevelChanged -= Show;
    }

    private void Show(int level)
    {
        _label.text = $"LEVEL  {level}";
    }
}
