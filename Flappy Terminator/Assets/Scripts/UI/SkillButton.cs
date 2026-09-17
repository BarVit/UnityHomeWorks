using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;
    [SerializeField] private float _emptyAlpha = 0.2f;
    [SerializeField] private float _loadedAlpha = 0.85f;

    private void Start()
    {
        SetLoaded(false);
    }

    public void SetLoaded(bool isLoaded)
    {
        _button.interactable = isLoaded;

        Color color = _icon.color;
        color.a = isLoaded ? _loadedAlpha : _emptyAlpha;
        _icon.color = color;
    }
}
