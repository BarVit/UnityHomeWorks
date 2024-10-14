using UnityEngine;

public class Target4 : Target
{
    private void Awake()
    {
        Waypoints = new()
        {
            new Vector3(30, 0.5f, 20),
            new Vector3(5, 0.5f , 50)
        };

        Speed = 2;
    }
}