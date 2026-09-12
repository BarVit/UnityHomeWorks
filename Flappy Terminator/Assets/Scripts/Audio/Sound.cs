using System;
using UnityEngine;

[Serializable]
public struct Sound
{
    [SerializeField] private AudioClip _clip;
    [SerializeField, Range(0f, 1f)] private float _volume;

    public AudioClip Clip => _clip;
    public float Volume => _volume;
}
