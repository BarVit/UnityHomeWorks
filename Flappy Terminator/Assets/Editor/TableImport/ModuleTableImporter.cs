using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TableImport
{
    public static class ModuleTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/modules.tsv";
        public const string AssetPath = "Assets/Data/Generated/Modules.asset";
        public const string UpgradesSourcePath = "Assets/Data/Tables/module_upgrades.tsv";
        public const string UpgradesAssetPath = "Assets/Data/Generated/ModuleUpgrades.asset";

        private const int MinUpgrades = 1;
        private const int MaxUpgrades = 3;

        private static readonly string[] Columns =
        {
            "id", "title", "kind", "price", "icon", "description", "unlock_after"
        };

        public static ModuleTable Import(LocationTable locations, TableLog log)
        {
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null || document.HasColumns(Columns, log) == false)
                return null;

            List<ModuleRow> rows = new List<ModuleRow>();
            HashSet<string> ids = new HashSet<string>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                string id = reader.ReadText("id");

                if (ids.Add(id) == false)
                    reader.Fail("повторяющийся id " + id);

                rows.Add(new ModuleRow(
                    id,
                    reader.ReadText("title"),
                    reader.ReadEnum<ModuleKind>("kind"),
                    reader.ReadInt("price", 0, 100000000),
                    reader.ReadAsset<Sprite>("icon", false),
                    reader.ReadOptionalText("description"),
                    ReadUnlock(locations, reader)));
            }

            ModuleTable table = GeneratedAsset.LoadOrCreate<ModuleTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }

        private static string ReadUnlock(LocationTable locations, RowReader reader)
        {
            string unlockAfter = reader.ReadOptionalText("unlock_after");

            if (unlockAfter.Length > 0 && locations != null && locations.GetRow(unlockAfter) == null
                && locations.IsVoid(unlockAfter) == false)
                reader.Fail("unlock_after: нет локации или пустоты " + unlockAfter + " в " + LocationTableImporter.SourcePath);

            return unlockAfter;
        }

        public static ModuleUpgradeTable ImportUpgrades(ModuleTable modules, TableLog log)
        {
            ModuleUpgradeTable table = LevelTableImporter.Import<ModuleUpgradeTable, ModuleStat>(UpgradesSourcePath,
                UpgradesAssetPath, "module", "stat", module => CheckModule(modules, module), log);

            if (table == null || modules == null)
                return table;

            foreach (ModuleRow module in modules.Rows)
            {
                int count = table.GetUpgradableStats(module.Id).Count;

                if (count < MinUpgrades || count > MaxUpgrades)
                    log.Add(UpgradesSourcePath, 0, "у модуля " + module.Id + " " + count
                        + " улучшений, нужно от " + MinUpgrades + " до " + MaxUpgrades);
            }

            return table;
        }

        private static string CheckModule(ModuleTable modules, string module)
        {
            if (module.Length == 0)
                return "пустой id модуля";

            if (modules == null || modules.GetRow(module) != null)
                return null;

            return "нет модуля с id " + module + " в " + SourcePath;
        }
    }
}
