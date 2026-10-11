namespace TableImport
{
    public static class SkillTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/skills.tsv";
        public const string AssetPath = "Assets/Data/Generated/Skills.asset";

        public static SkillTable Import(TableLog log)
        {
            return LevelTableImporter.Import<SkillTable, SkillStat>(SourcePath, AssetPath, null, "skill", null, log);
        }
    }
}
