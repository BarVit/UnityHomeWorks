using UnityEngine;

public class ImpulseDelivery : ProjectileDelivery
{
    private const int BoltsPerShot = 2;
    private const float GapShare = 2f;

    private Sound _echo;
    private float _echoLeft;

    public ImpulseDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
    }

    protected override void OnFire()
    {
        for (int barrel = 0; barrel < Barrels; barrel++)
            Volley(barrel);
    }

    protected override void OnTick(float deltaTime)
    {
        if (_echoLeft <= 0f)
            return;

        _echoLeft -= deltaTime;

        if (_echoLeft <= 0f)
            Context.Audio.Play(_echo, GetMount(0).position);
    }

    private void Volley(int barrel)
    {
        Quaternion rotation = GetRotation(barrel);
        Vector3 step = rotation * Vector3.right * GetSpacing();

        for (int bolt = 0; bolt < BoltsPerShot; bolt++)
        {
            Ammo shot = bolt == 0 ? Launch(barrel) : Spawn(barrel);

            shot.transform.position += step * bolt;

            if (barrel == 0 && bolt > 0)
                Echo(shot.ShotSound, step.magnitude * bolt);
        }
    }

    private void Echo(Sound sound, float distance)
    {
        _echo = sound;
        _echoLeft = ProjectileSpeed <= 0f ? 0f : distance / ProjectileSpeed;

        if (_echoLeft <= 0f)
            Context.Audio.Play(sound, GetMount(0).position);
    }

    private float GetSpacing()
    {
        Sprite sprite = Row.Projectile == null ? null : GetSprite();

        return sprite == null ? 1f : sprite.bounds.size.x * GapShare;
    }

    private Sprite GetSprite()
    {
        SpriteRenderer renderer = Row.Projectile.GetComponentInChildren<SpriteRenderer>();

        return renderer == null ? null : renderer.sprite;
    }
}
