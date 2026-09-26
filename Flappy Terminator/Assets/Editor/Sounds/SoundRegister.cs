using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace SoundContent
{
    public static class SoundRegister
    {
        private const string SoundsFolder = "Assets/Sounds";
        private const string OutputPath = "Docs/Sounds.md";
        private const string FreesoundUrl = "https://freesound.org/s/";
        private const string AssetsPrefix = "Assets/";

        private static readonly Regex IdPattern = new Regex(@"\[(\d+)\]");

        [MenuItem("Tools/Sounds/Write Register")]
        public static void Write()
        {
            List<string> clips = CollectClips();
            Dictionary<string, List<string>> users = CollectUsers(clips);
            HashSet<string> inGame = CollectInGame();

            File.WriteAllText(OutputPath, Format(clips, users, inGame), new UTF8Encoding(false));
            Debug.Log("Реестр звуков записан: " + OutputPath + ", звуков " + clips.Count);
        }

        private static List<string> CollectClips()
        {
            List<string> clips = new List<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new string[] { SoundsFolder }))
                clips.Add(AssetDatabase.GUIDToAssetPath(guid));

            clips.Sort();

            return clips;
        }

        private static Dictionary<string, List<string>> CollectUsers(List<string> clips)
        {
            Dictionary<string, List<string>> users = new Dictionary<string, List<string>>();

            foreach (string clip in clips)
                users[clip] = new List<string>();

            foreach (string guid in AssetDatabase.FindAssets(string.Empty, new string[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (AssetDatabase.IsValidFolder(path) || users.ContainsKey(path))
                    continue;

                foreach (string dependency in AssetDatabase.GetDependencies(path, false))
                {
                    if (users.TryGetValue(dependency, out List<string> found))
                        found.Add(path);
                }
            }

            return users;
        }

        private static HashSet<string> CollectInGame()
        {
            HashSet<string> reachable = new HashSet<string>();

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled == false)
                    continue;

                foreach (string dependency in AssetDatabase.GetDependencies(scene.path, true))
                    reachable.Add(dependency);
            }

            return reachable;
        }

        private static string Format(List<string> clips, Dictionary<string, List<string>> users, HashSet<string> inGame)
        {
            StringBuilder text = new StringBuilder();

            text.AppendLine("# Звуки проекта");
            text.AppendLine();
            text.AppendLine("[← Происхождение графики и звука](Assets.md)");
            text.AppendLine();
            text.AppendLine("Файл собирается командой `Tools/Sounds/Write Register` и руками не правится.");
            text.AppendLine("Номер в квадратных скобках — номер звука на Freesound, по нему звук находится");
            text.AppendLine("и в библиотеке, и на сайте. «В игре» — звук достижим из сцен сборки.");
            text.AppendLine();
            text.AppendLine("| Файл | Freesound | Длина | Размер | В игре | Где используется |");
            text.AppendLine("|---|---|---|---|---|---|");

            List<string> unused = new List<string>();

            foreach (string clip in clips)
            {
                bool isInGame = inGame.Contains(clip);

                if (isInGame == false)
                    unused.Add(clip);

                text.AppendLine("| `" + Path.GetFileName(clip) + "` | " + DescribeSource(clip) + " | "
                    + DescribeLength(clip) + " | " + DescribeSize(clip) + " | " + (isInGame ? "да" : "**нет**")
                    + " | " + DescribeUsers(users[clip]) + " |");
            }

            text.AppendLine();
            text.AppendLine("Всего звуков: " + clips.Count + ", из них в игре не используются: " + unused.Count + ".");

            return text.ToString();
        }

        private static string DescribeSource(string clip)
        {
            Match match = IdPattern.Match(Path.GetFileNameWithoutExtension(clip));

            if (match.Success == false)
                return "—";

            string id = match.Groups[1].Value;

            return "[" + id + "](" + FreesoundUrl + id + ")";
        }

        private static string DescribeLength(string clip)
        {
            AudioClip audio = AssetDatabase.LoadAssetAtPath<AudioClip>(clip);

            if (audio == null)
                return "—";

            string format = audio.length < 1f ? "0.00" : "0.0";

            return audio.length.ToString(format, CultureInfo.InvariantCulture) + " с";
        }

        private static string DescribeSize(string clip)
        {
            long bytes = new FileInfo(clip).Length;

            if (bytes >= 1024 * 1024)
                return (bytes / (1024f * 1024f)).ToString("0.0", CultureInfo.InvariantCulture) + " МБ";

            return (bytes / 1024) + " КБ";
        }

        private static string DescribeUsers(List<string> users)
        {
            if (users.Count == 0)
                return "—";

            List<string> names = new List<string>();

            foreach (string user in users)
                names.Add("`" + (user.StartsWith(AssetsPrefix) ? user.Substring(AssetsPrefix.Length) : user) + "`");

            names.Sort();

            return string.Join(", ", names);
        }
    }
}
