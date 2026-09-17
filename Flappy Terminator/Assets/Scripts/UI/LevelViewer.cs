using TMPro;
using UnityEngine;

public class LevelViewer : MonoBehaviour
{
    [SerializeField] private LocationRunner _levels;
    [SerializeField] private TMP_Text _label;

    private void OnEnable()
    {
        _levels.LevelChanged += Show;
    }

    private void OnDisable()
    {
        _levels.LevelChanged -= Show;
    }

    private void Show(int level)
    {
        _label.text = $"LEVEL  {level}";
    }
}
