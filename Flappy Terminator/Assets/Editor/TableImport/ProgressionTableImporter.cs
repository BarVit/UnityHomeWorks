using System.Collections.Generic;
using UnityEditor;

namespace TableImport
{
    public static class ProgressionTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/progression.tsv";
        public const string AssetPath = "Assets/Data/Generated/Progression.asset";

        private static readonly string[] Columns =
        {
            "id", "pilot_level", "pilot_xp_required", "ship_xp_required"
        };

        public static ProgressionTable Import(TableLog log)
        {
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null)
                return null;

            if (document.HasColumns(Columns, log) == false)
                return null;

            List<ProgressionRow> rows = new List<ProgressionRow>();
            HashSet<string> ids = new HashSet<string>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                string id = reader.ReadText("id");

                if (ids.Add(id) == false)
                    reader.Fail("повторяющийся id " + id);

                rows.Add(new ProgressionRow(
                    id,
                    reader.ReadInt("pilot_level", 1, 99),
                    reader.ReadInt("pilot_xp_required", 0, 1000000),
                    reader.ReadInt("ship_xp_required", 0, 1000000)));
            }

            ProgressionTable table = GeneratedAsset.LoadOrCreate<ProgressionTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }
    }
}
