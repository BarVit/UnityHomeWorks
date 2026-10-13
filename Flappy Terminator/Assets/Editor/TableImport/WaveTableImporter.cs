using System.Collections.Generic;
using UnityEditor;

namespace TableImport
{
    public static class WaveTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/waves.tsv";
        public const string AssetPath = "Assets/Data/Generated/Waves.asset";

        private const char ListSeparator = ',';

        private static readonly string[] Columns =
        {
            "location", "level", "order", "count", "mode", "enemies", "interval", "interval_max"
        };

        public static WaveTable Import(LocationTable locations, EnemyTable enemies, BossTable bosses, TableLog log)
        {
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null)
                return null;

            if (document.HasColumns(Columns, log) == false)
                return null;

            List<WaveRow> rows = new List<WaveRow>();
            HashSet<string> steps = new HashSet<string>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                string location = reader.ReadText("location");
                int level = reader.ReadInt("level", 1, 20);
                int order = reader.ReadInt("order", 1, 99);
                int count = reader.ReadInt("count", 1, 1000);
                WaveMode mode = reader.ReadEnum<WaveMode>("mode");
                string[] list = Split(reader.ReadText("enemies"));

                if (steps.Add(location + " " + level + " " + order) == false)
                    reader.Fail("у локации " + location + " на уровне " + level + " уже есть группа " + order);

                CheckLocation(locations, location, level, reader);
                CheckEnemies(enemies, bosses, location, mode, count, list, reader);

                float interval = reader.ReadFloat("interval", 0f, 60f);
                float intervalMax = reader.ReadFloat("interval_max", 0f, 60f);

                if (intervalMax < interval)
                    reader.Fail("interval_max: " + intervalMax + " меньше interval " + interval);

                rows.Add(new WaveRow(location, level, order, count, mode, list, interval, intervalMax));
            }

            WaveTable table = GeneratedAsset.LoadOrCreate<WaveTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }

        private static string[] Split(string text)
        {
            string[] parts = text.Split(ListSeparator);

            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Trim();

            return parts;
        }

        private static void CheckLocation(LocationTable locations, string location, int level, RowReader reader)
        {
            if (locations == null || location.Length == 0)
                return;

            LocationRow row = locations.GetRow(location);

            if (row == null)
                reader.Fail("location: нет локации с id " + location + " в " + LocationTableImporter.SourcePath);
            else if (level > row.Levels)
                reader.Fail("level: " + level + " больше, чем уровней у " + location + " (" + row.Levels + ")");
        }

        private static void CheckBoss(BossRow boss, string id, string location, RowReader reader)
        {
            if (boss == null)
                reader.Fail("enemies: нет босса с id " + id + " в " + BossTableImporter.SourcePath);
            else if (boss.Location != location)
                reader.Fail("enemies: босс " + id + " стоит в " + location + ", а в " + BossTableImporter.SourcePath
                    + " у него location «" + boss.Location + "»");
        }

        private static void CheckEnemies(EnemyTable enemies, BossTable bosses, string location, WaveMode mode,
            int count, string[] list, RowReader reader)
        {
            if (mode == WaveMode.Boss)
            {
                if (count != 1 || list.Length != 1)
                    reader.Fail("Boss: нужен ровно один босс и count 1");

                if (bosses != null)
                    CheckBoss(bosses.GetRow(list[0]), list[0], location, reader);

                return;
            }

            if (enemies == null)
                return;

            foreach (string enemy in list)
            {
                if (enemies.GetRow(enemy) == null)
                    reader.Fail("enemies: нет врага с id " + enemy + " в " + EnemyTableImporter.SourcePath);
            }
        }
    }
}
