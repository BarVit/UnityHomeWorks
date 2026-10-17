using System.Collections.Generic;

namespace TableImport
{
    public static class EnemyShieldTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/enemy_shields.tsv";

        private static readonly string[] Columns = { "level", "capacity" };

        public static Dictionary<int, int> Read(TableLog log)
        {
            Dictionary<int, int> capacities = new Dictionary<int, int>();
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null || document.HasColumns(Columns, log) == false)
                return capacities;

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                int level = reader.ReadInt("level", 1, 100);
                int capacity = reader.ReadInt("capacity", 1, 1000000);

                if (capacities.ContainsKey(level))
                    reader.Fail("повторяющийся уровень щита " + level);
                else
                    capacities.Add(level, capacity);
            }

            return capacities;
        }
    }
}
