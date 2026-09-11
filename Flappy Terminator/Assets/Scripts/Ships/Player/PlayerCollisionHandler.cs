using System;
using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    [SerializeField] private ParticleSystem _bumpParticleSystem;

    public event Action<Asteroid> AsteroidBumped;
    public event Action<EnemyShip> EnemyBumped;

    private void OnValidate()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Asteroid asteroid))
        {
            AsteroidBumped?.Invoke(asteroid);
            var bump = Instantiate(_bumpParticleSystem, other.ClosestPoint(transform.position), Quaternion.identity);
            var main = bump.main;

            if (transform.position.y > 0)
                main.gravityModifier = -2;
            else
                main.gravityModifier = 4;
            //проиграть звук удара об астероид

        }

        if (other.TryGetComponent(out EnemyCollision enemyCollision))
        {
            EnemyBumped?.Invoke(enemyCollision.GetShip());
            var bump = Instantiate(_bumpParticleSystem, other.ClosestPoint(transform.position), Quaternion.identity);
            var main = bump.main;
            main.gravityModifier = 1;
            enemyCollision.Bump();
            //проиграть звук удара о врага
        }
    }
}