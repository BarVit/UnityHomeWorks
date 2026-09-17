using System.Collections.Generic;
using UnityEditor;

namespace TableImport
{
    public static class SystemTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/systems.tsv";
        public const string AssetPath = "Assets/Data/Generated/Systems.asset";

        private static readonly string[] Columns = { "id", "title", "order" };

        public static SystemTable Import(TableLog log)
        {
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null)
                return null;

            if (document.HasColumns(Columns, log) == false)
                return null;

            List<SystemRow> rows = new List<SystemRow>();
            HashSet<string> ids = new HashSet<string>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                string id = reader.ReadText("id");

                if (ids.Add(id) == false)
                    reader.Fail("повторяющийся id " + id);

                rows.Add(new SystemRow(id, reader.ReadText("title"), reader.ReadInt("order", 1, 99)));
            }

            SystemTable table = GeneratedAsset.LoadOrCreate<SystemTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }
    }
}
