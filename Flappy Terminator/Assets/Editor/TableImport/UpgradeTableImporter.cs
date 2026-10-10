using System.Collections.Generic;
using UnityEditor;

namespace TableImport
{
    public static class UpgradeTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/weapon_upgrades.tsv";
        public const string AssetPath = "Assets/Data/Generated/WeaponUpgrades.asset";

        private static readonly string[] Columns =
        {
            "stat", "level", "bonus", "price"
        };

        public static UpgradeTable Import(TableLog log)
        {
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null)
                return null;

            if (document.HasColumns(Columns, log) == false)
                return null;

            List<UpgradeRow> rows = new List<UpgradeRow>();
            Dictionary<UpgradeStat, int> lastLevels = new Dictionary<UpgradeStat, int>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                UpgradeStat stat = reader.ReadEnum<UpgradeStat>("stat");
                int level = reader.ReadInt("level", 0, 50);
                int expected = lastLevels.TryGetValue(stat, out int last) ? last + 1 : 0;

                if (level != expected)
                    reader.Fail("level: у " + stat + " ждали уровень " + expected + ", уровни идут подряд с 0");

                lastLevels[stat] = level;

                rows.Add(new UpgradeRow(
                    stat,
                    level,
                    reader.ReadFloat("bonus", 0f, 100000f),
                    reader.ReadInt("price", 0, 10000000)));
            }

            UpgradeTable table = GeneratedAsset.LoadOrCreate<UpgradeTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }
    }
}
