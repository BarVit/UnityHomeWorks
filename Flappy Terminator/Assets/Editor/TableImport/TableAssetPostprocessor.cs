using UnityEditor;

namespace TableImport
{
    public class TableAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted,
            string[] moved, string[] movedFrom)
        {
            if (Touches(imported) || Touches(deleted) || Touches(moved))
                EditorApplication.delayCall += Reimport;
        }

        private static void Reimport()
        {
            TableImporter.Import();
        }

        private static bool Touches(string[] paths)
        {
            foreach (string path in paths)
            {
                if (path.StartsWith(TableImporter.TablesFolder) && path.EndsWith(".tsv"))
                    return true;
            }

            return false;
        }
    }
}
