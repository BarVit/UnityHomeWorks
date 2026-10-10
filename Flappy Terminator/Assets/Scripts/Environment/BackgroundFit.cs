using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFit : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Camera _camera;
    private Vector3 _baseScale;
    private float _fittedAspect = -1f;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _camera = Camera.main;
        _baseScale = transform.localScale;
    }

    private void LateUpdate()
    {
        if (Mathf.Approximately(_camera.aspect, _fittedAspect))
            return;

        _fittedAspect = _camera.aspect;
        Fit();
    }

    private void Fit()
    {
        Vector2 size = _renderer.sprite.bounds.size;
        float viewHeight = _camera.orthographicSize * 2f;
        float viewWidth = viewHeight * _camera.aspect;
        float cover = Mathf.Max(viewWidth / (size.x * _baseScale.x), viewHeight / (size.y * _baseScale.y), 1f);

        transform.localScale = new Vector3(_baseScale.x * cover, _baseScale.y * cover, _baseScale.z);
    }
}
