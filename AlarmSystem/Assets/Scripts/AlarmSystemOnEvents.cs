using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(HouseOnEvents))]
public class AlarmSystemOnEvents : MonoBehaviour
{
    private AudioSource _audioSource;
    private HouseOnEvents _houseOnEvents;

    private float _minVolume = 0;
    private float _maxVolume = 1;
    private float _changeRate = 0.3f;
    private bool _isAlarming = false;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _houseOnEvents = GetComponent<HouseOnEvents>();
        _audioSource.volume = _minVolume;
    }

    private void OnEnable()
    {
        _houseOnEvents.ThiefEntered += PlaySirene;
        _houseOnEvents.ThiefExited += StopSirene;
    }

    private void OnDisable()
    {
        _houseOnEvents.ThiefEntered -= PlaySirene;
        _houseOnEvents.ThiefExited -= StopSirene;
    }

    private void Update()
    {
        if(_isAlarming)
            _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, _maxVolume, _changeRate * Time.deltaTime);
        else
            _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, _minVolume, _changeRate * Time.deltaTime);

        if (_isAlarming == false && _audioSource.volume == 0)
            _audioSource.Stop();
    }

    private void PlaySirene()
    {
        _audioSource.volume = _minVolume;
        _isAlarming = true;
        _audioSource.Play();
    }

    private void StopSirene()
    {
        _isAlarming = false;
    }
}