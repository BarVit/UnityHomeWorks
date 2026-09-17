using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace FlappyBridge
{
    public static class ViewCapture
    {
        public static string Capture(string path, int width, int height, bool withInterface)
        {
            Camera camera = FindCamera();

            if (camera == null)
                return Json.Error("no enabled camera in the open scene");

            RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            target.Create();

            Texture2D image = null;
            CanvasAttachment attachment = new CanvasAttachment();
            int attached = 0;

            try
            {
                if (withInterface && Application.isPlaying)
                {
                    attachment.Attach(camera);
                    attached = attachment.Count;
                }

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
                attachment.Restore();

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
                Json.Field("playing", Application.isPlaying),
                Json.Field("canvases", attached),
                Json.Field("note", withInterface && Application.isPlaying == false
                    ? "overlay UI is only composited while playing, so this frame is world only"
                    : string.Empty));
        }

        public static string CaptureGameView(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (File.Exists(path))
                File.Delete(path);

            ScreenCapture.CaptureScreenshot(path);

            return Json.Object(
                "\"ok\":true",
                Json.Field("pending", true),
                Json.Field("path", path),
                Json.Field("playing", Application.isPlaying),
                Json.Field("note", "the game view is written at the end of the frame and includes overlay UI; "
                    + "wait for the file to appear"));
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
