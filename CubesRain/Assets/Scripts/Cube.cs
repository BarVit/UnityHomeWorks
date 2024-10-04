using UnityEngine;

[RequireComponent(typeof(Color))] 
public class Cube : MonoBehaviour
{
    private Color _color;

    private bool _isFirstCollision = true;

    private void Start()
    {
        _color = GetComponent<Color>();
        _color.SetColor(UnityEngine.Color.blue);
    }

    public void ChangeColor()
    {
        float minDestroyTime = 2f;
        float maxDestroyTime = 5f;

        if(_isFirstCollision)
        {
            _isFirstCollision = false;

            _color.SetColor(Random.ColorHSV());
            Destroy(gameObject, Random.Range(minDestroyTime, maxDestroyTime));
        }
    }
}