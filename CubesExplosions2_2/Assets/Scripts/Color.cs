using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Color : MonoBehaviour
{
    private Material _material;

    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
    }

    public void SetColor(UnityEngine.Color color)
    {
        _material.color = color;
    }
}