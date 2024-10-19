using UnityEngine;
using System.Collections.Generic;

public class Thief : MonoBehaviour
{
    private List<Vector3> _waypoints;
    private Vector3 _firstHousePosition;
    private Vector3 _secondHousePositon;
    private Vector3 _waypoint;
    private float _speed = 2;
    private float _thiefHeight = 1;
    private int _index = 0;

    private void Start()
    {
        _firstHousePosition = new Vector3(-7, _thiefHeight, 0);
        _secondHousePositon = new Vector3(21, _thiefHeight, 0);

        _waypoints = new()
        {
            _firstHousePosition,
            _secondHousePositon
        };

        _waypoint = _waypoints[_index];
        transform.forward = _waypoint - transform.position;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _waypoint, _speed * Time.deltaTime);

        if (transform.position == _waypoint)
            _waypoint = GetNextWaypoint();
    }

    private Vector3 GetNextWaypoint()
    {
        Vector3 nextPosition;

        _index = ++_index % _waypoints.Count;
        nextPosition = _waypoints[_index];
        transform.forward = nextPosition - transform.position;

        return nextPosition;
    }
}