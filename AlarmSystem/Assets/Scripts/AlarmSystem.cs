using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AlarmSystem : MonoBehaviour
{
    private AudioSource _audioSource;

    private float _minVolume = 0;
    private float _maxVolume = 1;
    private float _changeRate = 0.3f;

    private bool _isAlarming = false;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.volume = _minVolume;
    }

    private void Update()
    {
        if(_isAlarming)
            _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, _maxVolume, _changeRate * Time.deltaTime);
        else
            _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, _minVolume, _changeRate * Time.deltaTime);

        if (_isAlarming == false && _audioSource.volume == 0)
            _audioSource.Stop();

        Debug.Log(_audioSource.volume);
    }

    public void PlaySirene()
    {
        _audioSource.volume = _minVolume;
        _isAlarming = true;
        _audioSource.Play();
    }

    public void StopSirene()
    {
        _isAlarming = false;
    }
}