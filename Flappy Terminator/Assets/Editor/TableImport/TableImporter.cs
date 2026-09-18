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
                ProgressionTableImporter.Import(log);
                WeaponTable weapons = WeaponTableImporter.Import(log);
                EnemyShipTable ships = EnemyShipTableImporter.Import(weapons, locations, log);
                EnemyTierTableImporter.Import(ships, weapons, log);

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
