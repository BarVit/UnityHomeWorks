public class EnemyArmament
{
    public EnemyArmament(WeaponRow weapon, int barrels, int damage, float rateScale)
    {
        Weapon = weapon;
        Barrels = barrels;
        Damage = damage;
        RateScale = rateScale;
    }

    public WeaponRow Weapon { get; private set; }

    public int Barrels { get; private set; }

    public int Damage { get; private set; }

    public float RateScale { get; private set; }
}
