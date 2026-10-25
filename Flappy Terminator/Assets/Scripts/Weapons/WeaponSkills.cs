public readonly struct WeaponSkills
{
    public static readonly WeaponSkills Default = new WeaponSkills(1f, 0f, 0, 0f, true, 1f, 1f);

    public WeaponSkills(float rateScale, float reloadShift, int magazineBonus, float speedShift, bool hasSight,
        float heatPoolScale, float coolingTimeScale)
    {
        RateScale = rateScale;
        ReloadShift = reloadShift;
        MagazineBonus = magazineBonus;
        SpeedShift = speedShift;
        HasSight = hasSight;
        HeatPoolScale = heatPoolScale;
        CoolingTimeScale = coolingTimeScale;
    }

    public float RateScale { get; }

    public float ReloadShift { get; }

    public int MagazineBonus { get; }

    public float SpeedShift { get; }

    public bool HasSight { get; }

    public float HeatPoolScale { get; }

    public float CoolingTimeScale { get; }
}
