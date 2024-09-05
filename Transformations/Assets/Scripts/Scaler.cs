using UnityEngine;

public class Scaler : MonoBehaviour
{
    [SerializeField] private float _speed = 0.5f;

    private void Update()
    {
        float scale = _speed * Time.deltaTime;

        transform.localScale += new Vector3(scale, scale, scale);
    }
}