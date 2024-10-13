using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoPlaces : MonoBehaviour
{
    [SerializeField] private Transform _placesArea;
    [SerializeField] private float _speed;

    private Transform[] _places;

    private int _currentPlaceNumber = 0;

    private void Start()
    {
        _places = new Transform[_placesArea.childCount];

        for (int i = 0; i < _placesArea.childCount; i++)
        {
            _places[i] = _placesArea.GetChild(i).GetComponent<Transform>();
        }

    private void Update()
    {
        Vector3 placePosition = _places[_currentPlaceNumber].position;

        transform.position = Vector3.MoveTowards(transform.position, placePosition, _speed * Time.deltaTime);
        
        if (transform.position == placePosition)
            transform.forward = NextPlace() - transform.position;
    }

    private Vector3 NextPlace()
    {
        if (_currentPlaceNumber++ == _places.Length)
            _currentPlaceNumber = 0;

        return _places[_currentPlaceNumber].transform.position;
    }
}