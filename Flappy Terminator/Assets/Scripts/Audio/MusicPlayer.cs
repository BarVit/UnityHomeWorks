using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip[] _tracks;
    [SerializeField] private float _crossfadeDuration = 3f;

    private AudioSource _current;
    private AudioSource _next;
    private float _volume;
    private int _trackIndex;

    private void Awake()
    {
        _current = GetComponent<AudioSource>();
        _current.ignoreListenerPause = true;
        _volume = _current.volume;
        _next = CreateSecondSource(_current);
    }

    private void Start()
    {
        if (_tracks.Length == 0)
            return;

        _current.loop = _tracks.Length == 1;
        Play(_current, _tracks[_trackIndex], _volume);
    }

    private void Update()
    {
        if (_tracks.Length < 2)
            return;

        if (_next.isPlaying && _current.isPlaying == false)
        {
            Swap();

            return;
        }

        float remaining = _current.clip.length - _current.time;

        if (_next.isPlaying == false)
        {
            if (remaining > _crossfadeDuration)
                return;

            _trackIndex = (_trackIndex + 1) % _tracks.Length;
            Play(_next, _tracks[_trackIndex], 0f);
        }

        Blend(1f - Mathf.Clamp01(remaining / _crossfadeDuration));
    }

    private void Blend(float progress)
    {
        float angle = progress * Mathf.PI * 0.5f;

        _current.volume = _volume * Mathf.Cos(angle);
        _next.volume = _volume * Mathf.Sin(angle);
    }

    private void Swap()
    {
        AudioSource finished = _current;

        _current = _next;
        _next = finished;
        _current.volume = _volume;
        _next.volume = 0f;
    }

    private void Play(AudioSource source, AudioClip track, float volume)
    {
        source.clip = track;
        source.time = 0f;
        source.volume = volume;
        source.Play();
    }

    private AudioSource CreateSecondSource(AudioSource original)
    {
        AudioSource second = gameObject.AddComponent<AudioSource>();

        second.outputAudioMixerGroup = original.outputAudioMixerGroup;
        second.priority = original.priority;
        second.pitch = original.pitch;
        second.spatialBlend = original.spatialBlend;
        second.ignoreListenerPause = true;
        second.playOnAwake = false;
        second.loop = false;

        return second;
    }
}
