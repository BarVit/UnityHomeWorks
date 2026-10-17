using System.Collections.Generic;

namespace TableImport
{
    public static class EnemyShieldTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/enemy_shields.tsv";

        private static readonly string[] Columns = { "level", "capacity", "reward_money", "reward_xp" };

        public static Dictionary<int, ShieldLevel> Read(TableLog log)
        {
            Dictionary<int, ShieldLevel> levels = new Dictionary<int, ShieldLevel>();
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null || document.HasColumns(Columns, log) == false)
                return levels;

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                int level = reader.ReadInt("level", 1, 100);
                ShieldLevel shield = new ShieldLevel(
                    reader.ReadInt("capacity", 1, 1000000),
                    new Reward(reader.ReadInt("reward_xp", 0, 1000000), reader.ReadInt("reward_money", 0, 1000000)));

                if (levels.ContainsKey(level))
                    reader.Fail("повторяющийся уровень щита " + level);
                else
                    levels.Add(level, shield);
            }

            return levels;
        }
    }
}
