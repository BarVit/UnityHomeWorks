using TMPro;
using UnityEngine;

public class MagazineLabel : MonoBehaviour
{
    [SerializeField] private PlayerShooter _shooter;
    [SerializeField] private TMP_Text _label;

    private void OnEnable()
    {
        _shooter.ShotsLeftChanged += Show;
        _shooter.WeaponChanged += Refresh;
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        _shooter.ShotsLeftChanged -= Show;
        _shooter.WeaponChanged -= Refresh;
    }

    private void Refresh()
    {
        _label.enabled = _shooter.HasMagazine;
        Show(_shooter.ShotsLeft);
    }

    private void Show(int shotsLeft)
    {
        _label.text = shotsLeft + " / " + _shooter.MagazineSize;
    }
}
