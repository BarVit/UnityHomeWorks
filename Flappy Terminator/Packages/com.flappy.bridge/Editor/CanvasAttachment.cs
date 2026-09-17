using System.Collections.Generic;
using UnityEngine;

namespace FlappyBridge
{
    public class CanvasAttachment
    {
        private readonly List<Canvas> _canvases = new List<Canvas>();
        private readonly List<RenderMode> _modes = new List<RenderMode>();
        private readonly List<Camera> _cameras = new List<Camera>();
        private readonly List<float> _distances = new List<float>();

        public int Count
        {
            get { return _canvases.Count; }
        }

        public void Attach(Camera camera)
        {
            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    continue;

                _canvases.Add(canvas);
                _modes.Add(canvas.renderMode);
                _cameras.Add(canvas.worldCamera);
                _distances.Add(canvas.planeDistance);

                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = Mathf.Clamp(1f, camera.nearClipPlane + 0.01f, camera.farClipPlane - 0.01f);
            }

            Canvas.ForceUpdateCanvases();
        }

        public void Restore()
        {
            for (int i = 0; i < _canvases.Count; i++)
            {
                if (_canvases[i] == null)
                    continue;

                _canvases[i].renderMode = _modes[i];
                _canvases[i].worldCamera = _cameras[i];
                _canvases[i].planeDistance = _distances[i];
            }

            _canvases.Clear();
            _modes.Clear();
            _cameras.Clear();
            _distances.Clear();

            Canvas.ForceUpdateCanvases();
        }
    }
}
