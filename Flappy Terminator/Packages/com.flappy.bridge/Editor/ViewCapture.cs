using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace FlappyBridge
{
    public static class ViewCapture
    {
        public static string Capture(string path, int width, int height)
        {
            Camera camera = FindCamera();

            if (camera == null)
                return Json.Error("no enabled camera in the open scene");

            RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            target.Create();

            Texture2D image = null;

            try
            {
                Render(camera, target);

                image = new Texture2D(width, height, TextureFormat.RGB24, false);

                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                RenderTexture.active = previous;

                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                target.Release();
                Object.DestroyImmediate(target);

                if (image != null)
                    Object.DestroyImmediate(image);
            }

            return Json.Object(
                "\"ok\":true",
                Json.Field("path", path),
                Json.Field("camera", camera.name),
                Json.Field("width", width),
                Json.Field("height", height),
                Json.Field("playing", Application.isPlaying));
        }

        public static string GetDefaultPath()
        {
            string root = Directory.GetParent(Application.dataPath).FullName;

            return Path.Combine(root, "Temp", "bridge", "shot.png");
        }

        private static Camera FindCamera()
        {
            if (Camera.main != null)
                return Camera.main;

            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);

            foreach (Camera camera in cameras)
            {
                if (camera.isActiveAndEnabled)
                    return camera;
            }

            return null;
        }

        private static void Render(Camera camera, RenderTexture target)
        {
            RenderPipeline.StandardRequest request = new RenderPipeline.StandardRequest();
            request.destination = target;

            if (RenderPipeline.SupportsRenderRequest(camera, request))
            {
                RenderPipeline.SubmitRenderRequest(camera, request);

                return;
            }

            RenderTexture previous = camera.targetTexture;
            camera.targetTexture = target;
            camera.Render();
            camera.targetTexture = previous;
        }
    }
}
