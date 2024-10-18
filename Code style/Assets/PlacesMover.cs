using UnityEngine;

public class GoPlaces : MonoBehaviour
{
    [SerializeField] private Transform _placesArea;
    [SerializeField] private float _speed;

    private Transform[] _places;
    private Transform _place;
    private int _index;

    private void Start()
    {
        _places = new Transform[_placesArea.childCount];

        for (int i = 0; i < _places.Length; i++)
            _places[i] = _placesArea.GetChild(i);

        _place = _places[_index];
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _place.position, _speed * Time.deltaTime);

        if (transform.position == _place.position)
            GetNextPlace();
    }

    private Vector3 GetNextPlace()
    {
        Vector3 nextPosition;

        if (++_index == _places.Length)
            _index = 0;

        nextPosition = _places[_index].transform.position;
        transform.forward = nextPosition - transform.position;
        return nextPosition;
    }
}