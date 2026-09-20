using UnityEngine;

public class ImpulseDelivery : ProjectileDelivery
{
    private const int BoltsPerShot = 2;

    public ImpulseDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
    }

    protected override void OnFire()
    {
        for (int barrel = 0; barrel < Barrels; barrel++)
            Volley(barrel);
    }

    private void Volley(int barrel)
    {
        Quaternion rotation = GetRotation(barrel);
        Vector3 back = rotation * Vector3.left * GetSpacing();

        for (int bolt = 0; bolt < BoltsPerShot; bolt++)
        {
            Ammo shot = Launch(barrel);

            shot.transform.position += back * bolt;
        }
    }

    private float GetSpacing()
    {
        Sprite sprite = Row.Projectile == null ? null : GetSprite();

        return sprite == null ? 1f : sprite.bounds.size.x * 1.05f;
    }

    private Sprite GetSprite()
    {
        SpriteRenderer renderer = Row.Projectile.GetComponentInChildren<SpriteRenderer>();

        return renderer == null ? null : renderer.sprite;
    }
}
