using UnityEngine;

public class ShootCooldownBar : MonoBehaviour
{
    [SerializeField] private PlayerShooter _shooter;
    [SerializeField] private RectTransform _fill;

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
    }
}
