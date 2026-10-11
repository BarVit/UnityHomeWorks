namespace TableImport
{
    public static class UpgradeTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/weapon_upgrades.tsv";
        public const string AssetPath = "Assets/Data/Generated/WeaponUpgrades.asset";

        public static UpgradeTable Import(WeaponTable weapons, TableLog log)
        {
            return LevelTableImporter.Import<UpgradeTable, UpgradeStat>(SourcePath, AssetPath, "weapon", "stat",
                weapon => CheckWeapon(weapons, weapon), log);
        }

        private static string CheckWeapon(WeaponTable weapons, string weapon)
        {
            if (weapons == null || weapon.Length == 0)
                return null;

            WeaponRow row = weapons.GetRow(weapon);

            if (row == null)
                return "нет оружия с id " + weapon + " в " + WeaponTableImporter.SourcePath;

            return row.Owner == WeaponOwner.Player ? null : weapon + " не пушка игрока";
        }
    }
}
