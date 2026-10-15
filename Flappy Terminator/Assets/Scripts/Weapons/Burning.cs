using System.Collections.Generic;
using UnityEngine;

public class Burning : MonoBehaviour
{
    private const float Duration = 1.2f;
    private const float TickInterval = 0.2f;

    private readonly List<float> _damagePerSecond = new List<float>();
    private readonly List<float> _timeLeft = new List<float>();

    private IDamageable _target;
    private ParticleSystem _flame;
    private float _pendingDamage;
    private float _tickLeft;

    public static void Ignite(GameObject struck, float damage, ParticleSystem flamePrefab)
    {
        if (struck == null || damage <= 0f)
            return;

        Component target = struck.GetComponentInParent<IDamageable>() as Component;

        if (target == null)
            return;

        if (target.TryGetComponent(out Burning burning) == false)
            burning = target.gameObject.AddComponent<Burning>();

        burning.Add(damage, flamePrefab);
    }

    private void Awake()
    {
        _target = GetComponent<IDamageable>();
    }

    private void Update()
    {
        if (_timeLeft.Count == 0)
            return;

        float deltaTime = Time.deltaTime;

        for (int i = _timeLeft.Count - 1; i >= 0; i--)
        {
            float burnTime = Mathf.Min(deltaTime, _timeLeft[i]);

            _pendingDamage += _damagePerSecond[i] * burnTime;
            _timeLeft[i] -= burnTime;

            if (_timeLeft[i] > 0f)
                continue;

            _timeLeft.RemoveAt(i);
            _damagePerSecond.RemoveAt(i);
        }

        _tickLeft -= deltaTime;

        if (_timeLeft.Count == 0)
        {
            Deal(Mathf.RoundToInt(_pendingDamage));
            Extinguish();

            return;
        }

        if (_tickLeft <= 0f)
        {
            _tickLeft = TickInterval;
            Deal(Mathf.FloorToInt(_pendingDamage));
        }
    }

    private void OnDisable()
    {
        _timeLeft.Clear();
        _damagePerSecond.Clear();
        Extinguish();
    }

    private void Add(float damage, ParticleSystem flamePrefab)
    {
        if (_timeLeft.Count == 0)
            _tickLeft = TickInterval;

        _damagePerSecond.Add(damage / Duration);
        _timeLeft.Add(Duration);

        if (_flame == null)
            _flame = Instantiate(flamePrefab, transform);

        if (_flame.isPlaying == false)
            _flame.Play(true);
    }

    private void Deal(int damage)
    {
        if (damage <= 0)
            return;

        _pendingDamage -= damage;

        if (_target != null && _target.IsVulnerable)
            _target.TakeDamage(damage);
    }

    private void Extinguish()
    {
        _pendingDamage = 0f;

        if (_flame != null)
            _flame.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
