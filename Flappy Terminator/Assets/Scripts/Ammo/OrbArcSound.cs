using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OrbArcSound : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 0.1f;

    private AudioSource _source;
    private float _volume;
    private bool _isTouching;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _volume = _source.volume;
        _source.loop = true;
        _source.playOnAwake = false;
    }

    private void OnEnable()
    {
        _isTouching = false;
        _source.Stop();
    }

    private void Update()
    {
        if (_isTouching)
        {
            Sound();

            return;
        }

        FadeOut();
    }

    public void SetTouching(bool isTouching)
    {
        _isTouching = isTouching;
    }

    private void Sound()
    {
        if (_source.isPlaying == false)
        {
            _source.time = 0f;
            _source.Play();
        }

        _source.volume = _volume;
    }

    private void FadeOut()
    {
        if (_source.isPlaying == false)
            return;

        float step = _fadeDuration <= 0f ? _volume : _volume / _fadeDuration * Time.deltaTime;

        _source.volume = Mathf.MoveTowards(_source.volume, 0f, step);

        if (_source.volume <= 0f)
            _source.Stop();
    }
}
