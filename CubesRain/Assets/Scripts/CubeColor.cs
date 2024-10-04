using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class CubeColor : MonoBehaviour
{
    private Material _material;

    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
    }

    public void SetColor(Color color)
    {
        _material.color = color;
    }
}