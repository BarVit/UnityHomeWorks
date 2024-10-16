using UnityEngine;

public class Door : MonoBehaviour
{
    private bool _isInside = false;

    private void OnTriggerEnter(Collider other)
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (transform.position.x > other.transform.position.x)
            _isInside = true;
        else
            _isInside = false;
    }

    private void Update()
    {
        Debug.Log(_isInside);
    }
}