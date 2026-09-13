using TMPro;
using UnityEngine;

public class LevelViewer : MonoBehaviour
{
    [SerializeField] private LevelProgression _levels;
    [SerializeField] private TMP_Text _label;

    private void OnEnable()
    {
        _levels.Changed += Show;
    }

    private void OnDisable()
    {
        _levels.Changed -= Show;
    }

    private void Show(int level)
    {
        _label.text = $"LEVEL  {level}";
    }
}
