using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] private int _speed = 1;

    private void Update()
    {
        transform.position += transform.forward * _speed * Time.deltaTime;
    }
}