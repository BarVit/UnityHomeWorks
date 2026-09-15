using UnityEngine;

public static class ScreenArea
{
    private const float AudibleMargin = 1.5f;

    public static bool IsAudible(Camera camera, Vector3 position)
    {
        return Contains(camera, position, AudibleMargin);
    }

    public static bool Contains(Camera camera, Vector3 position)
    {
        return Contains(camera, position, 0f);
    }

    public static bool Contains(Camera camera, Vector3 position, float margin)
    {
        if (camera == null)
            return true;

        Vector3 viewportPoint = camera.WorldToViewportPoint(position);
        float horizontalMargin = margin / (GetHalfWidth(camera) * 2f);
        float verticalMargin = margin / (camera.orthographicSize * 2f);

        return viewportPoint.z > 0f
            && viewportPoint.x >= -horizontalMargin && viewportPoint.x <= 1f + horizontalMargin
            && viewportPoint.y >= -verticalMargin && viewportPoint.y <= 1f + verticalMargin;
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
