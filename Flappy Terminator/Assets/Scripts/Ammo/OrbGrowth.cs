using UnityEngine;

public class OrbGrowth : MonoBehaviour
{
    [SerializeField] private float _startScale = 0.594f;
    [SerializeField] private float _endScale = 1.5625f;

    private Camera _camera;
    private float _sizeScale = 1f;
    private float _startPositionX;
    private float _endPositionX;
    private float _chargeDuration;
    private float _chargeLeft;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void OnEnable()
    {
        _chargeLeft = 0f;
        _startPositionX = transform.position.x;
        _endPositionX = ScreenArea.GetRightEdge(_camera);
        Show(0f);
    }

    private void Update()
    {
        if (_chargeLeft > 0f)
        {
            _chargeLeft -= Time.deltaTime;
            ShowCharge();

            if (_chargeLeft <= 0f)
                _startPositionX = transform.position.x;

            return;
        }

        Show(GetProgress());
    }

    public void SetSize(float scale)
    {
        _sizeScale = Mathf.Max(0f, scale);
    }

    public void Charge(float duration)
    {
        _chargeDuration = duration;
        _chargeLeft = duration;
        ShowCharge();
    }

    private float GetProgress()
    {
        float span = _endPositionX - _startPositionX;

        if (span <= 0f)
            return 1f;

        return Mathf.Clamp01((transform.position.x - _startPositionX) / span);
    }

    private void ShowCharge()
    {
        float share = _chargeDuration <= 0f ? 1f : 1f - Mathf.Clamp01(_chargeLeft / _chargeDuration);

        SetScale(_startScale * _sizeScale * share);
    }

    private void Show(float progress)
    {
        SetScale(Mathf.Lerp(_startScale, _endScale, progress) * _sizeScale);
    }

    private void SetScale(float scale)
    {
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
