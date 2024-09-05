using UnityEngine;

public class Rotater : MonoBehaviour
{
    [SerializeField] private int _speed = 30;

    private void Update()
    {
        transform.Rotate(0, _speed * Time.deltaTime, 0);
    }
}