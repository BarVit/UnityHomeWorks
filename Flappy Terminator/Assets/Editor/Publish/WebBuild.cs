using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BuildTools
{
    public static class WebBuild
    {
        private const string OutputRoot = "Builds/Web";
        private const string FirstVersion = "0.1.0";
        private const int ScreenWidth = 1280;
        private const int ScreenHeight = 720;

        [MenuItem("Tools/Build/Web")]
        public static void Build()
        {
            ConfigurePlayer();

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

            string previous = PlayerSettings.bundleVersion;
            string version = RaiseVersion();
            string path = Path.Combine(OutputRoot, "v" + version);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = GetScenes(),
                locationPathName = path,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
                Debug.Log("Сборка Web v" + version + " готова: " + Path.GetFullPath(path)
                    + " (" + summary.totalSize / (1024 * 1024) + " МБ, " + summary.totalTime.TotalSeconds.ToString("0") + " с)");
            else
                Fail(version, previous, summary);
        }

        private static void Fail(string version, string previous, BuildSummary summary)
        {
            PlayerSettings.bundleVersion = previous;
            AssetDatabase.SaveAssets();
            Debug.LogError("Сборка Web v" + version + " не удалась: " + summary.result + ", ошибок " + summary.totalErrors
                + ". Версия возвращена на " + previous);
        }

        [MenuItem("Tools/Build/Apply Web Settings")]
        public static void ConfigurePlayer()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.defaultWebScreenWidth = ScreenWidth;
            PlayerSettings.defaultWebScreenHeight = ScreenHeight;

            if (IsVersion(PlayerSettings.bundleVersion) == false)
                PlayerSettings.bundleVersion = FirstVersion;

            AssetDatabase.SaveAssets();
        }

        private static string RaiseVersion()
        {
            string[] parts = PlayerSettings.bundleVersion.Split('.');

            parts[2] = (int.Parse(parts[2]) + 1).ToString();
            PlayerSettings.bundleVersion = string.Join(".", parts);
            AssetDatabase.SaveAssets();

            return PlayerSettings.bundleVersion;
        }

        private static bool IsVersion(string text)
        {
            string[] parts = text.Split('.');

            if (parts.Length != 3)
                return false;

            foreach (string part in parts)
            {
                if (int.TryParse(part, out _) == false)
                    return false;
            }

            return true;
        }

        private static string[] GetScenes()
        {
            List<string> scenes = new List<string>();

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }

            return scenes.ToArray();
        }
    }
}
