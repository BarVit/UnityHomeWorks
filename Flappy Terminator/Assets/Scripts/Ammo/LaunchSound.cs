using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LaunchSound : MonoBehaviour
{
    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.loop = false;
        _source.playOnAwake = false;
    }

    private void OnDisable()
    {
        _source.Stop();
    }

    public void Play(Sound sound)
    {
        if (sound.Clip == null)
            return;

        _source.clip = sound.Clip;
        _source.volume = sound.Volume;
        _source.Play();
    }
}
