using System;
using UnityEngine;

[Serializable]
public class WeaponAssets
{
    [SerializeField] private Sprite _icon;
    [SerializeField] private Sprite _projectileSprite;
    [SerializeField] private AnimationClip _projectileAnimation;
    [SerializeField] private ExplosionAnimation _shotEffect;
    [SerializeField] private ExplosionAnimation _hitTargetEffect;
    [SerializeField] private ExplosionAnimation _hitWorldEffect;
    [SerializeField] private AudioClip _shotSound;
    [SerializeField] private AudioClip _flightSound;
    [SerializeField] private AudioClip _hitTargetSound;
    [SerializeField] private AudioClip _hitWorldSound;
    [SerializeField] private AudioClip _overheatSound;

    public WeaponAssets(Sprite icon, Sprite projectileSprite, AnimationClip projectileAnimation,
        ExplosionAnimation shotEffect, ExplosionAnimation hitTargetEffect, ExplosionAnimation hitWorldEffect,
        AudioClip shotSound, AudioClip flightSound, AudioClip hitTargetSound, AudioClip hitWorldSound,
        AudioClip overheatSound)
    {
        _icon = icon;
        _projectileSprite = projectileSprite;
        _projectileAnimation = projectileAnimation;
        _shotEffect = shotEffect;
        _hitTargetEffect = hitTargetEffect;
        _hitWorldEffect = hitWorldEffect;
        _shotSound = shotSound;
        _flightSound = flightSound;
        _hitTargetSound = hitTargetSound;
        _hitWorldSound = hitWorldSound;
        _overheatSound = overheatSound;
    }

    public Sprite Icon => _icon;

    public Sprite ProjectileSprite => _projectileSprite;

    public AnimationClip ProjectileAnimation => _projectileAnimation;

    public ExplosionAnimation ShotEffect => _shotEffect;

    public ExplosionAnimation HitTargetEffect => _hitTargetEffect;

    public ExplosionAnimation HitWorldEffect => _hitWorldEffect;

    public AudioClip ShotSound => _shotSound;

    public AudioClip FlightSound => _flightSound;

    public AudioClip HitTargetSound => _hitTargetSound;

    public AudioClip HitWorldSound => _hitWorldSound;

    public AudioClip OverheatSound => _overheatSound;
}
