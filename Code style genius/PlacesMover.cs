using UnityEngine;

public class PlacesMover : MonoBehaviour
{
    [SerializeField] private Transform _placesArea;
    [SerializeField] private float _speed;

    private Transform[] _places;
    private Vector3 _placePosition;
    private int _index;

    private void Start()
    {
        _places = new Transform[_placesArea.childCount];

        for (int i = 0; i < _places.Length; i++)
            _places[i] = _placesArea.GetChild(i);

        _placePosition = _places[_index].position;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _placePosition, _speed * Time.deltaTime);

        if (transform.position == _placePosition)
            _placePosition = GetNextPosition();
    }

    private Vector3 GetNextPosition()
    {
        Vector3 nextPosition;

        _index = ++_index % _places.Length;

        nextPosition = _places[_index].transform.position;
        transform.forward = nextPosition - transform.position;
        return nextPosition;
    }
}