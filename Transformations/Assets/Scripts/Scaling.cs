using UnityEngine;

public class Scaling : MonoBehaviour
{
    [SerializeField] private int _speed = 3;
    private int _scaleChanger = -1;

    private float _maxScale = 3;
    private float _minScale = 0.5f;

    private void Update()
    {
        float scale;

        if (transform.localScale.x > _maxScale && _speed > 0)
            _speed *= _scaleChanger;
        else if (transform.localScale.x < _minScale && _speed < 0)
            _speed *= _scaleChanger;

        scale = _speed * Time.deltaTime;
        transform.localScale += new Vector3(scale, scale, scale);
    }
}