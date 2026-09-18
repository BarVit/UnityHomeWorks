using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public event Action Pressed;
    public event Action Released;

    public void OnPointerDown(PointerEventData eventData)
    {
        Pressed?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Released?.Invoke();
    }

    private void OnDisable()
    {
        Released?.Invoke();
    }
}
