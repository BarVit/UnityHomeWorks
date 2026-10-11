using UnityEngine;

public class MapOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private float _fadeDuration = 0.15f;

    private float _target;

    private void Update()
    {
        _group.alpha = Mathf.MoveTowards(_group.alpha, _target, Time.unscaledDeltaTime / _fadeDuration);
    }

    public void Show(bool isShown)
    {
        _target = isShown ? 1f : 0f;
    }

    public void HideNow()
    {
        _target = 0f;
        _group.alpha = 0f;
    }
}
