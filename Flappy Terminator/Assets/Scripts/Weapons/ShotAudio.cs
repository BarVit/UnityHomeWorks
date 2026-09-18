using UnityEngine;

public class ShotAudio
{
    private readonly AudioSource _source;
    private readonly Camera _camera;

    public ShotAudio(AudioSource source, Camera camera)
    {
        _source = source;
        _camera = camera;
    }

    public void Play(Sound sound, Vector3 position)
    {
        if (sound.Clip == null)
            return;

        if (ScreenArea.IsAudible(_camera, position) == false)
            return;

        _source.PlayOneShot(sound.Clip, sound.Volume);
    }
}
