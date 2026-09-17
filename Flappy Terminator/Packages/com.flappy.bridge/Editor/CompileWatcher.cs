using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;

namespace FlappyBridge
{
    [InitializeOnLoad]
    public static class CompileWatcher
    {
        private const string MessagesKey = "FlappyBridge.CompileMessages";
        private const string StateKey = "FlappyBridge.CompileState";

        static CompileWatcher()
        {
            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.compilationStarted += OnCompilationStarted;

            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyFinished;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyFinished;

            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
        }

        public static string GetMessages()
        {
            return "[" + SessionState.GetString(MessagesKey, string.Empty) + "]";
        }

        public static int GetErrorCount()
        {
            return Count(SessionState.GetString(MessagesKey, string.Empty), "\"type\":\"Error\"");
        }

        public static int GetWarningCount()
        {
            return Count(SessionState.GetString(MessagesKey, string.Empty), "\"type\":\"Warning\"");
        }

        public static string GetState()
        {
            return SessionState.GetString(StateKey, "unknown");
        }

        private static void OnCompilationStarted(object context)
        {
            SessionState.SetString(MessagesKey, string.Empty);
            SessionState.SetString(StateKey, "running");
        }

        private static void OnCompilationFinished(object context)
        {
            SessionState.SetString(StateKey, "finished");
        }

        private static void OnAssemblyFinished(string assemblyPath, CompilerMessage[] messages)
        {
            List<string> collected = new List<string>();

            foreach (CompilerMessage message in messages)
            {
                if (message.type != CompilerMessageType.Error && message.type != CompilerMessageType.Warning)
                    continue;

                collected.Add(Json.Object(
                    Json.Field("type", message.type.ToString()),
                    Json.Field("file", message.file),
                    Json.Field("line", message.line),
                    Json.Field("message", message.message)));
            }

            if (collected.Count == 0)
                return;

            string stored = SessionState.GetString(MessagesKey, string.Empty);
            string added = string.Join(",", collected);

            SessionState.SetString(MessagesKey, stored.Length == 0 ? added : stored + "," + added);
        }

        private static int Count(string source, string token)
        {
            int count = 0;
            int index = source.IndexOf(token, System.StringComparison.Ordinal);

            while (index >= 0)
            {
                count++;
                index = source.IndexOf(token, index + token.Length, System.StringComparison.Ordinal);
            }

            return count;
        }
    }
}
