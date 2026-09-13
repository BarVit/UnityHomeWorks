using UnityEngine;

public class PauseController : MonoBehaviour
{
    private const float NormalTimeScale = 1f;
    private const float PausedTimeScale = 0f;

    [SerializeField] private PlayerShip _playerShip;

    private bool _wasControlEnabled;

    public bool IsPaused { get; private set; }

    public void Pause()
    {
        IsPaused = true;
        _wasControlEnabled = _playerShip.IsControlEnabled;
        Time.timeScale = PausedTimeScale;
        AudioListener.pause = true;
        _playerShip.LockInput();
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = NormalTimeScale;
        AudioListener.pause = false;

        if (_wasControlEnabled)
            _playerShip.UnlockInput();
    }
}
