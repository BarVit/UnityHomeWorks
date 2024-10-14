using UnityEngine;

public class Target1 : Target
{
    private void Awake()
    {
        Waypoints = new()
        {
            new Vector3(-30, 0.5f, -30),
            new Vector3(-30, 0.5f , -50),
            new Vector3(-50, 0.5f , -50),
            new Vector3(-50, 0.5f , -30)
        };

        Speed = 6;
    }
}