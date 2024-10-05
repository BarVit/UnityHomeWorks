using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    private Color _originalColor = Color.blue;

    public void SetRandomColor(Material material)
    {
        material.color = Random.ColorHSV();
    }

    public void SetOriginalColor(Material material)
    {
        material.color = _originalColor;
    }
}