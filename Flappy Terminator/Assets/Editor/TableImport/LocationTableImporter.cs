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
            "id", "system", "order", "levels", "boss", "border_theme", "flight_mode", "title", "branch_of", "void"
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
                    reader.ReadOptionalText("boss"),
                    reader.ReadOptionalText("border_theme"),
                    reader.ReadEnum<FlightMode>("flight_mode"),
                    reader.ReadOptionalText("title"),
                    reader.ReadOptionalText("branch_of"),
                    reader.ReadOptionalText("void")));
            }

            CheckBranches(rows, log);

            LocationTable table = GeneratedAsset.LoadOrCreate<LocationTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }

        private static void CheckBranches(List<LocationRow> rows, TableLog log)
        {
            foreach (LocationRow row in rows)
            {
                if (row.IsInVoid && row.IsBranch == false)
                    log.Add(SourcePath, 0, row.Id + ": локация пустоты открывается вратами — нужен branch_of");

                if (row.IsBranch == false)
                    continue;

                LocationRow parent = rows.Find(other => other.Id == row.BranchOf);

                if (parent == null)
                    log.Add(SourcePath, 0, row.Id + ": branch_of " + row.BranchOf + " — нет такой локации");
                else if (parent.IsBranch || parent.System != row.System)
                    log.Add(SourcePath, 0, row.Id + ": ответвление должно идти от основной локации той же системы");

                if (row.IsInVoid && rows.Exists(other => other.Void == row.Void && other.BranchOf != row.BranchOf))
                    log.Add(SourcePath, 0, row.Id + ": локации одной пустоты должны отходить от одной локации (branch_of)");

                if (string.IsNullOrEmpty(row.Title))
                    log.Add(SourcePath, 0, row.Id + ": у ответвления нужно название в столбце title");
            }
        }
    }
}
