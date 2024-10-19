using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Signalization : MonoBehaviour
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
        if (_isAlarming)
            ChangeVolumeSmoothly(_maxVolume);
        else
            ChangeVolumeSmoothly(_minVolume);

        if (_isAlarming == false && _audioSource.volume == 0)
            _audioSource.Stop();
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

    private void ChangeVolumeSmoothly(float volume)
    {
        _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, volume, _changeRate * Time.deltaTime);
    }
}