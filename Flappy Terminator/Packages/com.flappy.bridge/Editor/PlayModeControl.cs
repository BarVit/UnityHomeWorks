using UnityEditor;
using UnityEngine;

namespace FlappyBridge
{
    public static class PlayModeControl
    {
        public static string Play()
        {
            if (Application.isPlaying)
                return Answer("already playing");

            EditorApplication.EnterPlaymode();

            return Answer("entering play mode; poll /status until playing is true");
        }

        public static string Stop()
        {
            if (Application.isPlaying == false)
                return Answer("already stopped");

            EditorApplication.ExitPlaymode();

            return Answer("leaving play mode; poll /status until playing is false");
        }

        public static string Pause(bool paused)
        {
            if (Application.isPlaying == false)
                return Json.Error("the editor is not playing");

            EditorApplication.isPaused = paused;

            return Answer(paused ? "paused" : "resumed");
        }

        public static string Step()
        {
            if (Application.isPlaying == false)
                return Json.Error("the editor is not playing");

            EditorApplication.Step();

            return Answer("stepped one frame");
        }

        private static string Answer(string note)
        {
            return Json.Object(
                "\"ok\":true",
                Json.Field("playing", Application.isPlaying),
                Json.Field("paused", EditorApplication.isPaused),
                Json.Field("note", note));
        }
    }
}
