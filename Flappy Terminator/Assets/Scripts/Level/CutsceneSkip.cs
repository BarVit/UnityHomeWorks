using UnityEngine;

public class CutsceneSkip : MonoBehaviour
{
    [SerializeField] private GameObject _hint;
    [SerializeField] private float _delay = 1f;

    private bool _isRunning;
    private int _startFrame;
    private float _startTime;

    public bool CanSkip => _isRunning && Time.frameCount > _startFrame && Time.time >= _startTime + _delay;

    private void Update()
    {
        if (CanSkip && _hint.activeSelf == false)
            _hint.SetActive(true);
    }

    public void Begin()
    {
        _isRunning = true;
        _startFrame = Time.frameCount;
        _startTime = Time.time;
        _hint.SetActive(false);
    }

    public void End()
    {
        _isRunning = false;
        _hint.SetActive(false);
    }
}
