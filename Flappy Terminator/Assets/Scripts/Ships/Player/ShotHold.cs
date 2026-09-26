using UnityEngine;

[RequireComponent(typeof(PlayerMover), typeof(PlayerShooter))]
public class ShotHold : MonoBehaviour
{
    private PlayerMover _mover;
    private PlayerShooter _shooter;
    private bool _wasHeld;

    private void Awake()
    {
        _mover = GetComponent<PlayerMover>();
        _shooter = GetComponent<PlayerShooter>();
    }

    private void OnDisable()
    {
        _wasHeld = false;
    }

    private void Update()
    {
        float holdLeft = _shooter.HoldLeft;
        bool isHeld = holdLeft > 0f;

        if (isHeld && _wasHeld == false && _mover.enabled)
            _mover.Hold(holdLeft, null);

        _wasHeld = isHeld;
    }
}
