using UnityEngine;

public class ExplosionAnimation : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    public void End(float time)
    {
        _audioSource.Play();
        Destroy(gameObject, time) ;
    }
}