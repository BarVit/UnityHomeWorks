public readonly struct SkillKey
{
    public SkillKey(SkillStat stat, string weapon)
    {
        Stat = stat;
        Weapon = weapon;
    }

    public SkillStat Stat { get; }

    public string Weapon { get; }

    public bool IsShipWide => string.IsNullOrEmpty(Weapon);

    public string StorageKey => IsShipWide ? Stat.ToString() : Weapon + "/" + Stat;
}
