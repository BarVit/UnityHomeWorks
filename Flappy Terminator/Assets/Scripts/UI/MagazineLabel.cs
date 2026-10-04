using TMPro;
using UnityEngine;

public class MagazineLabel : MonoBehaviour
{
    [SerializeField] private PlayerShooter _shooter;
    [SerializeField] private TMP_Text _label;

    private void OnEnable()
    {
        _shooter.ShotsLeftChanged += Show;
    }

    private void Start()
    {
        _label.enabled = _shooter.HasMagazine;
        Show(_shooter.ShotsLeft);
    }

    private void OnDisable()
    {
        _shooter.ShotsLeftChanged -= Show;
    }

    private void Show(int shotsLeft)
    {
        _label.text = shotsLeft + " / " + _shooter.MagazineSize;
    }
}
