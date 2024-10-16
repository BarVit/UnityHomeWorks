using System.Collections.Generic;
using UnityEngine;

public abstract class Target : MonoBehaviour
{
    protected List<Vector3> Waypoints;

    protected Vector3 Waypoint;
    protected int Index = 0;
    protected float Speed;

    public virtual void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, Waypoint, Speed * Time.deltaTime);

        if (transform.position == Waypoint)
            Waypoint = GetNextWaypoint();
    }

    protected Vector3 GetNextWaypoint()
    {
        if (++Index > Waypoints.Count - 1)
            Index = 0;

        return Waypoints[Index];
    }
}