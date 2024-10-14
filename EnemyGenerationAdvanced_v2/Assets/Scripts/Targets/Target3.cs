using UnityEngine;

public class Target3 : Target
{
    private void Awake()
    {
        Waypoints = new()
        {
            new Vector3(55, 0.5f, 10),
            new Vector3(35, 0.5f, 30),
            new Vector3(40, 0.5f, 10)
        };

        Speed = 4;
    }
}