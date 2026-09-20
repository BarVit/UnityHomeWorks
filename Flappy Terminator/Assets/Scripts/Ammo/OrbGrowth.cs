using UnityEngine;

public class OrbGrowth : MonoBehaviour
{
    [SerializeField] private float _startScale = 0.594f;
    [SerializeField] private float _endScale = 2.375f;

    private Camera _camera;
    private float _startPositionX;
    private float _endPositionX;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void OnEnable()
    {
        _startPositionX = transform.position.x;
        _endPositionX = ScreenArea.GetRightEdge(_camera);
        Show(0f);
    }

    private void Update()
    {
        Show(GetProgress());
    }

    private float GetProgress()
    {
        float span = _endPositionX - _startPositionX;

        if (span <= 0f)
            return 1f;

        return Mathf.Clamp01((transform.position.x - _startPositionX) / span);
    }

    private void Show(float progress)
    {
        float scale = Mathf.Lerp(_startScale, _endScale, progress);

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
