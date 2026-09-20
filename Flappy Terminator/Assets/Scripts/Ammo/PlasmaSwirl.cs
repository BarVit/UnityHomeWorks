using UnityEngine;

public class PlasmaSwirl : MonoBehaviour
{
    [SerializeField] private float _speed = 40f;

    private void Update()
    {
        transform.Rotate(0f, 0f, _speed * Time.deltaTime);
    }
}
