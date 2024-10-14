using UnityEngine;

public class Target5 : Target
{
    private void Awake()
    {
        Waypoints = new()
        {
            new Vector3(-55, 0.5f, 30),
            new Vector3(0, 0.5f , 45)
        };

        Speed = 5;
    }
}