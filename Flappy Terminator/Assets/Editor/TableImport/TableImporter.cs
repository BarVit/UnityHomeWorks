using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TableImport
{
    [InitializeOnLoad]
    public static class TableImporter
    {
        public const string TablesFolder = "Assets/Data/Tables";

        private static bool _isImporting;

        static TableImporter()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        public static bool Import()
        {
            if (_isImporting)
                return true;

            _isImporting = true;

            try
            {
                TableLog log = new TableLog();
                SystemTable systems = SystemTableImporter.Import(log);
                LocationTable locations = LocationTableImporter.Import(systems, log);
                WeaponTable weapons = WeaponTableImporter.Import(locations, log);
                EnemyTable enemies = EnemyTableImporter.Import(locations, weapons, log);
                BossTable bosses = BossTableImporter.Import(locations, enemies, weapons, log);
                Dictionary<int, int> shields = EnemyShieldTableImporter.Read(log);
                WaveTable waves = WaveTableImporter.Import(locations, enemies, bosses, shields, log);
                SlotTable slots = SlotTableImporter.Import(weapons, log);
                UpgradeTable upgrades = UpgradeTableImporter.Import(weapons, log);
                ModuleTable modules = ModuleTableImporter.Import(locations, log);
                ModuleTableImporter.ImportUpgrades(modules, log);
                SlotTableImporter.ImportModuleSlots(log);
                SkillTableImporter.Import(weapons, log);

                if (log.HasProblems)
                {
                    log.Report();

                    return false;
                }

                LocationRewardReport.Write(systems, locations, waves, enemies, bosses);
                WeaponFullPriceReport.Write(systems, locations, weapons, slots, upgrades);
                AssetDatabase.SaveAssets();

                return true;
            }
            finally
            {
                _isImporting = false;
            }
        }

        [MenuItem("Tools/Tables/Reimport All")]
        private static void ReimportAll()
        {
            if (Import())
                Debug.Log("Таблицы: импорт прошёл без ошибок.");
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode)
                return;

            if (Import())
                return;

            EditorApplication.isPlaying = false;
            Debug.LogError("Таблицы: вход в Play отменён, потому что данные содержат ошибки.");
        }
    }
}
