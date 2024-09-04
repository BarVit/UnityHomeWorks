using UnityEngine;

public class Moving : MonoBehaviour
{
    [SerializeField] private int _speed = 3;
    private int _directionChanger = -1;

    private float _movingDistance = 3;

    private void Update()
    {
        if (transform.position.z > _movingDistance && _speed > 0)
            _speed *= _directionChanger;
        else if (transform.position.z < -_movingDistance && _speed < 0)
            _speed *= _directionChanger;

        transform.position += new Vector3(0, 0, _speed * Time.deltaTime);
    }
}