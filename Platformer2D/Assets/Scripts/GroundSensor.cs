using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    private int _colliders = 0;

    public bool IsGrounded => _colliders == 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _colliders++;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _colliders--;
    }
}