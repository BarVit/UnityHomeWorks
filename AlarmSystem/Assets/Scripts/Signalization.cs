using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(AudioSource))]
public class Signalization : MonoBehaviour
{
    private AudioSource _audioSource;

    private float _smoothChangeDuration = 2f;
    private float _minVolume = 0;
    private float _maxVolume = 1;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.volume = _minVolume;
    }
    
    private IEnumerator ChangeVolumeSmoothly(float targetVolume)
    {
        float elapsedTime = 0f;
        float volumeDiaposone = Math.Abs(targetVolume - _audioSource.volume);
        float volumeAtStart = _audioSource.volume;
        float intermediateVolume;
        float deltaVolume;

        while(elapsedTime < _smoothChangeDuration)
        {
            elapsedTime += Time.deltaTime;
            deltaVolume = Time.deltaTime * volumeDiaposone / _smoothChangeDuration;

            if (targetVolume > _audioSource.volume)
                intermediateVolume = elapsedTime * volumeDiaposone / _smoothChangeDuration;
            else
                intermediateVolume = volumeAtStart - elapsedTime * volumeDiaposone / _smoothChangeDuration;

            _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, intermediateVolume, deltaVolume);

            yield return null;
        }

        if (_audioSource.isPlaying == true && _audioSource.volume == 0)
            _audioSource.Stop();
    }

    public void PlaySirene()
    {
        _audioSource.volume = _minVolume;
        _audioSource.Play();
        StartCoroutine(ChangeVolumeSmoothly(_maxVolume));
    }

    public void StopSirene()
    {
        StartCoroutine(ChangeVolumeSmoothly(_minVolume));
    }
}