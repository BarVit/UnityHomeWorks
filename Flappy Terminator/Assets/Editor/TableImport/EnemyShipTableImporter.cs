using System.Collections.Generic;
using UnityEditor;

namespace TableImport
{
    public static class EnemyShipTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/enemy_ships.tsv";
        public const string AssetPath = "Assets/Data/Generated/EnemyShips.asset";

        private static readonly string[] Columns =
        {
            "id", "title", "prefab", "body_damage", "from_location"
        };

        public static EnemyShipTable Import(LocationTable locations, TableLog log)
        {
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null)
                return null;

            if (document.HasColumns(Columns, log) == false)
                return null;

            List<EnemyShipRow> rows = new List<EnemyShipRow>();
            HashSet<string> ids = new HashSet<string>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                string id = reader.ReadText("id");

                if (ids.Add(id) == false)
                    reader.Fail("повторяющийся id " + id);

                string fromLocation = reader.ReadText("from_location");

                CheckLocation(locations, fromLocation, reader);

                rows.Add(new EnemyShipRow(
                    id,
                    reader.ReadText("title"),
                    reader.ReadPrefab<EnemyShip>("prefab", true),
                    reader.ReadInt("body_damage", 0, 1000),
                    fromLocation));
            }

            EnemyShipTable table = GeneratedAsset.LoadOrCreate<EnemyShipTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }

        private static void CheckLocation(LocationTable locations, string location, RowReader reader)
        {
            if (locations == null || location.Length == 0)
                return;

            if (locations.GetRow(location) == null)
                reader.Fail("from_location: нет локации с id " + location
                    + " в " + LocationTableImporter.SourcePath);
        }
    }
}
