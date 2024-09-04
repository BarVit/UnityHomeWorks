using UnityEngine;

public class Rotating : MonoBehaviour
{
    [SerializeField] private int _speed = 100;

    private void Update()
    {
        transform.Rotate(0, _speed * Time.deltaTime, 0);
    }
}