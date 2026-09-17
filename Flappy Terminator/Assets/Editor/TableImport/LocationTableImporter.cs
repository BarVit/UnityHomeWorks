using System.Collections.Generic;
using UnityEditor;

namespace TableImport
{
    public static class LocationTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/locations.tsv";
        public const string AssetPath = "Assets/Data/Generated/Locations.asset";

        private static readonly string[] Columns =
        {
            "id", "system", "order", "levels", "border_theme", "flight_mode",
            "first_wave_size", "wave_size_growth"
        };

        public static LocationTable Import(SystemTable systems, TableLog log)
        {
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null)
                return null;

            if (document.HasColumns(Columns, log) == false)
                return null;

            List<LocationRow> rows = new List<LocationRow>();
            HashSet<string> ids = new HashSet<string>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                string id = reader.ReadText("id");

                if (ids.Add(id) == false)
                    reader.Fail("повторяющийся id " + id);

                string system = reader.ReadText("system");

                if (systems != null && system.Length > 0 && systems.GetRow(system) == null)
                    reader.Fail("system: нет системы с id " + system + " в " + SystemTableImporter.SourcePath);

                rows.Add(new LocationRow(
                    id,
                    system,
                    reader.ReadInt("order", 1, 99),
                    reader.ReadInt("levels", 1, 20),
                    reader.ReadOptionalText("border_theme"),
                    reader.ReadEnum<FlightMode>("flight_mode"),
                    reader.ReadInt("first_wave_size", 1, 99),
                    reader.ReadInt("wave_size_growth", 0, 99)));
            }

            LocationTable table = GeneratedAsset.LoadOrCreate<LocationTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }
    }
}
