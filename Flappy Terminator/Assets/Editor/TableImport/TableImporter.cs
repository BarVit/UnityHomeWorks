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
                BossTable bosses = BossTableImporter.Import(enemies, weapons, log);
                WaveTableImporter.Import(locations, enemies, bosses, log);
                SlotTableImporter.Import(weapons, log);
                UpgradeTableImporter.Import(weapons, log);
                ModuleTable modules = ModuleTableImporter.Import(log);
                ModuleTableImporter.ImportUpgrades(modules, log);
                SlotTableImporter.ImportModuleSlots(log);
                SkillTableImporter.Import(log);

                if (log.HasProblems)
                {
                    log.Report();

                    return false;
                }

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
