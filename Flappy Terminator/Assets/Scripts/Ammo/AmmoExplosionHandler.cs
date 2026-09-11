using System.Collections.Generic;
using UnityEngine;

public class AmmoExplosionHandler : MonoBehaviour
{
    //[SerializeField] private List<ExplosionAnimation> _explosionAnimations;

    private ExplosionAnimation _explosionAnimation;

    public void PlayExplosionAnimation(ExplosionAnimation explosionAnimation)
    {
        _explosionAnimation = explosionAnimation;
    }
}