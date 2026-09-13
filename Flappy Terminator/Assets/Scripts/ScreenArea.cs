using UnityEngine;

public static class ScreenArea
{
    public static bool Contains(Camera camera, Vector3 position)
    {
        if (camera == null)
            return true;

        Vector3 viewportPoint = camera.WorldToViewportPoint(position);

        return viewportPoint.z > 0f
            && viewportPoint.x >= 0f && viewportPoint.x <= 1f
            && viewportPoint.y >= 0f && viewportPoint.y <= 1f;
    }

    public static float GetLeftEdge(Camera camera)
    {
        return camera.transform.position.x - GetHalfWidth(camera);
    }

    public static float GetRightEdge(Camera camera)
    {
        return camera.transform.position.x + GetHalfWidth(camera);
    }

    private static float GetHalfWidth(Camera camera)
    {
        return camera.orthographicSize * camera.aspect;
    }
}
