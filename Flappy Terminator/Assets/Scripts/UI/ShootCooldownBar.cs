using UnityEngine;
using UnityEngine.UI;

public class ShootCooldownBar : MonoBehaviour
{
    [SerializeField] private PlayerShooter _shooter;
    [SerializeField] private RectTransform _fill;
    [SerializeField] private Image _fillImage;

    private const float WarmReadiness = 0.7f;
    private const float HotReadiness = 0.4f;

    private static readonly Color CoolColor = new Color(0.25f, 1f, 0.35f);
    private static readonly Color WarmColor = new Color(1f, 0.92f, 0.2f);
    private static readonly Color HotColor = new Color(1f, 0.2f, 0.12f);

    private Color _plainColor;
    private bool _isPlainColorKnown;

    private void OnEnable()
    {
        _shooter.ReadinessChanged += Show;
    }

    private void Start()
    {
        Show(1f);
    }

    private void OnDisable()
    {
        _shooter.ReadinessChanged -= Show;
    }

    private void Show(float readiness)
    {
        Vector2 anchorMax = _fill.anchorMax;

        anchorMax.x = Mathf.Clamp01(readiness);
        _fill.anchorMax = anchorMax;

        Paint(readiness);
    }

    private void Paint(float readiness)
    {
        if (_fillImage == null)
            return;

        if (_isPlainColorKnown == false)
        {
            _plainColor = _fillImage.color;
            _isPlainColorKnown = true;
        }

        if (_shooter.HasHeat == false)
        {
            _fillImage.color = _plainColor;

            return;
        }

        _fillImage.color = GetHeatColor(readiness);
    }

    private Color GetHeatColor(float readiness)
    {
        if (readiness >= WarmReadiness)
            return Color.Lerp(WarmColor, CoolColor, (readiness - WarmReadiness) / (1f - WarmReadiness));

        if (readiness >= HotReadiness)
            return Color.Lerp(HotColor, WarmColor, (readiness - HotReadiness) / (WarmReadiness - HotReadiness));

        return HotColor;
    }
}
