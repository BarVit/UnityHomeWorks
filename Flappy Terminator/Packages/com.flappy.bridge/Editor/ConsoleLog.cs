using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FlappyBridge
{
    [InitializeOnLoad]
    public static class ConsoleLog
    {
        private const int Capacity = 300;

        private static readonly List<string> Entries = new List<string>();

        static ConsoleLog()
        {
            Application.logMessageReceivedThreaded -= Record;
            Application.logMessageReceivedThreaded += Record;
        }

        public static string GetEntries(int count, bool errorsOnly)
        {
            List<string> selected = new List<string>();

            lock (Entries)
            {
                for (int i = Entries.Count - 1; i >= 0 && selected.Count < count; i--)
                {
                    if (errorsOnly && IsFailure(Entries[i]) == false)
                        continue;

                    selected.Add(Entries[i]);
                }
            }

            selected.Reverse();

            return Json.Array(selected);
        }

        public static void Clear()
        {
            lock (Entries)
                Entries.Clear();
        }

        private static bool IsFailure(string entry)
        {
            return entry.Contains("\"type\":\"Error\"")
                || entry.Contains("\"type\":\"Exception\"")
                || entry.Contains("\"type\":\"Assert\"");
        }

        private static void Record(string message, string stackTrace, LogType type)
        {
            string entry = Json.Object(
                Json.Field("type", type.ToString()),
                Json.Field("message", message),
                Json.Field("stack", Shorten(stackTrace)));

            lock (Entries)
            {
                Entries.Add(entry);

                if (Entries.Count > Capacity)
                    Entries.RemoveAt(0);
            }
        }

        private static string Shorten(string stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
                return string.Empty;

            string[] lines = stackTrace.Split('\n');
            int taken = Mathf.Min(lines.Length, 4);

            return string.Join("\n", lines, 0, taken);
        }
    }
}
