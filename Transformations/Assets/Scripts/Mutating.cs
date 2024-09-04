using UnityEngine;

public class Mutating : MonoBehaviour
{
    [SerializeField] private int _movingSpeed = 3;
    [SerializeField] private int _scalingSpeed = 1;
    [SerializeField] private int _rotatingSpeed = 15;
    
    private void Update()
    {
        float scale = _scalingSpeed * Time.deltaTime;

        transform.position += transform.forward * Time.deltaTime * _movingSpeed;
        transform.localScale += new Vector3(scale, scale, scale);
        transform.Rotate(0, _rotatingSpeed * Time.deltaTime, 0);
    }
}