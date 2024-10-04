using UnityEngine;

public class Plane : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Cube cube = collision.collider.GetComponent<Cube>();
        
        if (cube != null)
            cube.ChangeColor();
    }
}