using UnityEngine;

public class SoundPlayer
{
    private AudioSource _audioSource;

    public SoundPlayer(AudioSource audioSource)
    {
        _audioSource = audioSource;
        _audioSource.Play();
    }

    //public void Play(AudioSource audioSource)
    //{
    //    AudioSource sound =  Instantiate(audioSource.GetComponent<AudioSource>());
        
    //    sound.Play();
    //}
}