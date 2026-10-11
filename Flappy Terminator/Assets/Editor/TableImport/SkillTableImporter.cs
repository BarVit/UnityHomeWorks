namespace TableImport
{
    public static class SkillTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/skills.tsv";
        public const string AssetPath = "Assets/Data/Generated/Skills.asset";

        public static SkillTable Import(WeaponTable weapons, TableLog log)
        {
            return LevelTableImporter.Import<SkillTable, SkillStat>(SourcePath, AssetPath, "weapon", "skill",
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
