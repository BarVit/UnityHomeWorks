using System.IO;
using System.Text;
using UnityEditor;

namespace TableImport
{
    public static class ReportFile
    {
        public const string Separator = "\t";
        public const string LineEnd = "\r\n";

        public static void Save(string path, string text)
        {
            string folder = Path.GetDirectoryName(path);

            if (Directory.Exists(folder) == false)
                Directory.CreateDirectory(folder);

            if (File.Exists(path) && File.ReadAllText(path, Encoding.Unicode) == text)
                return;

            File.WriteAllText(path, text, new UnicodeEncoding(false, true));
            AssetDatabase.ImportAsset(path);
        }
    }
}
