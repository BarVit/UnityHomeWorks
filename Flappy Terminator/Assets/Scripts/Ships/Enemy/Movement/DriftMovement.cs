using UnityEngine;

public class DriftMovement : EnemyMovement
{
    private float _phase;

    public DriftMovement(Rigidbody2D body, MovementSettings settings) : base(body, settings)
    {
    }

    public override void Tick(float deltaTime)
    {
        if (Settings.DriftPeriod <= 0f)
            return;

        _phase += deltaTime * 2f * Mathf.PI / Settings.DriftPeriod;

        Body.linearVelocity = (Vector2)(Body.transform.right * -Speed)
            + Vector2.up * (Mathf.Cos(_phase) * Settings.DriftAmplitude);
    }

    protected override void OnBegin()
    {
        _phase = Random.Range(0f, 2f * Mathf.PI);
        base.OnBegin();
    }
}
