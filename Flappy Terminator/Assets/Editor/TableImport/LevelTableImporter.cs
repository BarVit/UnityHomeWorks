using System;
using System.Collections.Generic;
using UnityEditor;

namespace TableImport
{
    public static class LevelTableImporter
    {
        private const string LevelColumn = "level";
        private const string BonusColumn = "bonus";
        private const string PriceColumn = "price";

        public static TTable Import<TTable, TStat>(string sourcePath, string assetPath, string ownerColumn,
            string statColumn, Func<string, string> checkOwner, TableLog log)
            where TTable : LevelTable<TStat>
            where TStat : struct, Enum
        {
            TsvDocument document = TsvReader.Read(sourcePath, log);

            if (document == null)
                return null;

            List<string> columns = new List<string> { statColumn, LevelColumn, BonusColumn, PriceColumn };

            if (ownerColumn != null)
                columns.Insert(0, ownerColumn);

            if (document.HasColumns(columns.ToArray(), log) == false)
                return null;

            List<LevelRow<TStat>> rows = new List<LevelRow<TStat>>();
            Dictionary<string, int> lastLevels = new Dictionary<string, int>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                string owner = ownerColumn == null ? LevelTable<TStat>.Everyone : reader.ReadOptionalText(ownerColumn);
                TStat stat = reader.ReadEnum<TStat>(statColumn);
                int level = reader.ReadInt(LevelColumn, 0, 100);
                string key = owner + "/" + stat;
                int expected = lastLevels.TryGetValue(key, out int last) ? last + 1 : 0;

                if (level != expected)
                    reader.Fail(LevelColumn + ": у " + DescribeKey(owner, stat) + " ждали уровень " + expected
                        + ", уровни идут подряд с 0");

                string problem = checkOwner?.Invoke(owner);

                if (problem != null)
                    reader.Fail(ownerColumn + ": " + problem);

                lastLevels[key] = level;

                rows.Add(new LevelRow<TStat>(
                    owner,
                    stat,
                    level,
                    reader.ReadFloat(BonusColumn, -100000f, 100000f),
                    reader.ReadInt(PriceColumn, 0, 100000000)));
            }

            TTable table = GeneratedAsset.LoadOrCreate<TTable>(assetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }

        private static string DescribeKey(string owner, object stat)
        {
            return owner.Length == 0 ? stat.ToString() : owner + " " + stat;
        }
    }
}
