using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Ammo : MonoBehaviour, IRemoveable
{
    [SerializeField] private ExplosionAnimation _prefab;
    [SerializeField] private AudioSource _audioShot;

    private SoundPlayer _soundPlayer;

    [field: SerializeField] public int Damage { get; private set; }

    public event Action<Ammo> Hitted;

    public void Remove()
    {
        Hitted?.Invoke(this);
    }

    public ExplosionAnimation GetExplosionAnimation()
    {
        return _prefab;
    }

    public void PlayShotSound()
    {
        //GameObject sound = new();

        //sound.AddComponent<AudioSource>();
        //AudioSource a = sound.GetComponent<AudioSource>();
        //a.clip = _audioSource.clip;
        //a.Play();
        _audioShot.Play();
    }

    public void Hit(Vector2 point)
    {
        var obj = Instantiate(_prefab, point, Quaternion.identity);
        obj.End(0.5f);
    }
}