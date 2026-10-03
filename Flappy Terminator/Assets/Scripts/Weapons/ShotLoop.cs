using UnityEngine;

public class ShotLoop
{
    private const float ReleaseDelay = 0.1f;
    private const float FadeDuration = 0.15f;

    private readonly AudioSource _source;

    private float _volume;
    private float _holdLeft;

    public ShotLoop(AudioSource source)
    {
        _source = source;
    }

    public void Hold(Sound sound)
    {
        if (_source == null || sound.Clip == null)
            return;

        _holdLeft = ReleaseDelay;
        _volume = sound.Volume;
        _source.volume = _volume;

        if (_source.isPlaying && _source.clip == sound.Clip)
            return;

        _source.clip = sound.Clip;
        _source.loop = true;
        _source.time = 0f;
        _source.Play();
    }

    public void Tick(float deltaTime)
    {
        if (_source == null || _source.isPlaying == false)
            return;

        if (_holdLeft > 0f)
        {
            _holdLeft -= deltaTime;

            return;
        }

        _source.volume = Mathf.MoveTowards(_source.volume, 0f, _volume / FadeDuration * deltaTime);

        if (_source.volume <= 0f)
            _source.Stop();
    }
}
