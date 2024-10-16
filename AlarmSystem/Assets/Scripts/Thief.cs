using UnityEngine;
using System.Collections.Generic;

public class Thief : MonoBehaviour
{
    private List<Vector3> _waypoints;
    private Vector3 _outDoorPositon;
    private Vector3 _inDoorPosition;
    private Vector3 _waypoint;
    private float _speed = 3;
    private float _thiefHeight = 1;
    private int _index = 0;

    private void Start()
    {
        _inDoorPosition = new Vector3(-7, _thiefHeight, 0);
        _outDoorPositon = new Vector3(7, _thiefHeight, 0);

        _waypoints = new()
        {
            _inDoorPosition,
            _outDoorPositon
        };

        _waypoint = _waypoints[_index];
        transform.forward = _waypoint - transform.position;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _waypoint, _speed * Time.deltaTime);

        if (transform.position == _waypoint)
            _waypoint = NextPosition();
    }

    private Vector3 NextPosition()
    {
        Vector3 nextPosition;

        if (++_index > _waypoints.Count - 1)
            _index = 0;

        nextPosition = _waypoints[_index];
        transform.forward = nextPosition - transform.position;
        return nextPosition;
    }
}