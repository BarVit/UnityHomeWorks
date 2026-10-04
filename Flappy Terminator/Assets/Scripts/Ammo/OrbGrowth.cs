using UnityEngine;

public class OrbGrowth : MonoBehaviour
{
    [SerializeField] private float _startScale = 0.594f;
    [SerializeField] private float _endScale = 1.875f;
    [SerializeField] private float _damageGrowth = 2.5f;
    [SerializeField] private float _maxDamage = 20f;

    private Camera _camera;
    private float _startPositionX;
    private float _endPositionX;
    private float _chargeDuration;
    private float _chargeLeft;
    private float _flightTime;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void OnEnable()
    {
        _chargeLeft = 0f;
        _flightTime = 0f;
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

        _flightTime += Time.deltaTime;
        Show(GetProgress());
    }

    public void Charge(float duration)
    {
        _chargeDuration = duration;
        _chargeLeft = duration;
        ShowCharge();
    }

    public float GetDamageRate(float startRate)
    {
        return Mathf.Min(startRate + _damageGrowth * _flightTime, _maxDamage);
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

        SetScale(_startScale * share);
    }

    private void Show(float progress)
    {
        SetScale(Mathf.Lerp(_startScale, _endScale, progress));
    }

    private void SetScale(float scale)
    {
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
