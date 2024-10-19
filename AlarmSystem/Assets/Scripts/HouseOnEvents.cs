using UnityEngine;
using System;

public class HouseOnEvents : MonoBehaviour
{
    public event Action ThiefEntered;
    public event Action ThiefExited;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Thief>() != null)
            ThiefEntered?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Thief>() != null)
            ThiefExited?.Invoke();
    }
}