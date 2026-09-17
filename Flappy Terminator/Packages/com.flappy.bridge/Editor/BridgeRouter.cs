using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FlappyBridge
{
    public static class BridgeRouter
    {
        public static string Route(Uri url)
        {
            Dictionary<string, string> query = ParseQuery(url.Query);

            switch (url.AbsolutePath)
            {
                case "/ping":
                    return Ping();

                case "/status":
                    return Status();

                case "/log":
                    return Log(query);

                case "/compile":
                    return Compile();

                case "/shot":
                    return Shot(query);

                case "/scene":
                    return SceneQuery.GetHierarchy(ReadString(query, "path", string.Empty),
                        ReadInt(query, "depth", 3));

                case "/object":
                    return SceneQuery.GetObject(ReadString(query, "path", string.Empty));

                case "/set":
                    return SceneEdit.SetProperty(
                        ReadString(query, "path", string.Empty),
                        ReadString(query, "component", string.Empty),
                        ReadString(query, "property", string.Empty),
                        ReadString(query, "value", string.Empty));

                case "/play":
                    return PlayModeControl.Play();

                case "/stop":
                    return PlayModeControl.Stop();

                case "/pause":
                    return PlayModeControl.Pause(ReadString(query, "value", "true") != "false");

                case "/step":
                    return PlayModeControl.Step();

                case "/clear":
                    ConsoleLog.Clear();

                    return "{\"ok\":true}";

                default:
                    return Json.Error("unknown route " + url.AbsolutePath + "; available: /ping /status /log "
                        + "/compile /shot /scene /object /set /play /stop /pause /step /clear");
            }
        }

        private static string Ping()
        {
            return Json.Object(
                "\"ok\":true",
                Json.Field("unity", Application.unityVersion),
                Json.Field("project", Application.productName),
                Json.Field("scene", EditorSceneManager.GetActiveScene().name),
                Json.Field("playing", Application.isPlaying),
                Json.Field("compiling", EditorApplication.isCompiling));
        }

        private static string Status()
        {
            return Json.Object(
                "\"ok\":true",
                Json.Field("playing", Application.isPlaying),
                Json.Field("paused", EditorApplication.isPaused),
                Json.Field("compiling", EditorApplication.isCompiling),
                Json.Field("updating", EditorApplication.isUpdating),
                Json.Field("scene", EditorSceneManager.GetActiveScene().name),
                Json.Field("sceneDirty", EditorSceneManager.GetActiveScene().isDirty),
                Json.Field("fastPlayMode", EditorSettings.enterPlayModeOptionsEnabled),
                Json.Field("playModeOptions", EditorSettings.enterPlayModeOptions.ToString()),
                Json.Field("compileState", CompileWatcher.GetState()),
                Json.Field("errors", CompileWatcher.GetErrorCount()),
                Json.Field("warnings", CompileWatcher.GetWarningCount()),
                Json.Raw("messages", CompileWatcher.GetMessages()));
        }

        private static string Log(Dictionary<string, string> query)
        {
            int count = ReadInt(query, "count", 50);
            bool errorsOnly = ReadString(query, "level", "all") == "error";

            return Json.Object(
                "\"ok\":true",
                Json.Raw("entries", ConsoleLog.GetEntries(count, errorsOnly)));
        }

        private static string Compile()
        {
            AssetDatabase.Refresh();
            CompilationPipeline.RequestScriptCompilation();

            return Json.Object(
                "\"ok\":true",
                Json.Field("requested", true),
                Json.Field("note", "a domain reload follows; poll /status until compileState is finished"));
        }

        private static string Shot(Dictionary<string, string> query)
        {
            string path = ReadString(query, "path", ViewCapture.GetDefaultPath());
            string mode = ReadString(query, "mode", "full");

            if (mode == "view")
                return ViewCapture.CaptureGameView(path);

            int width = ReadInt(query, "width", 960);
            int height = ReadInt(query, "height", 540);

            return ViewCapture.Capture(path, width, height, mode != "world");
        }

        private static Dictionary<string, string> ParseQuery(string query)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(query))
                return result;

            string[] pairs = query.TrimStart('?').Split('&');

            foreach (string pair in pairs)
            {
                if (pair.Length == 0)
                    continue;

                int split = pair.IndexOf('=');

                if (split < 0)
                    result[Uri.UnescapeDataString(pair)] = string.Empty;
                else
                    result[Uri.UnescapeDataString(pair.Substring(0, split))] =
                        Uri.UnescapeDataString(pair.Substring(split + 1));
            }

            return result;
        }

        private static string ReadString(Dictionary<string, string> query, string key, string fallback)
        {
            string value;

            if (query.TryGetValue(key, out value) && value.Length > 0)
                return value;

            return fallback;
        }

        private static int ReadInt(Dictionary<string, string> query, string key, int fallback)
        {
            string value;
            int parsed;

            if (query.TryGetValue(key, out value) && int.TryParse(value, out parsed))
                return parsed;

            return fallback;
        }
    }
}
