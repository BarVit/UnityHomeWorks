using UnityEngine;

public class GoPlaces : MonoBehaviour
{
    [SerializeField] private Transform _placesArea;
    [SerializeField] private float _speed;

    private Transform[] _places;
    private int _index;

    private void Start()
    {
        _places = new Transform[_placesArea.childCount];

        for (int i = 0; i < _places.Length; i++)
            _places[i] = _placesArea.GetChild(i);
    }

    private void Update()
    {
        Transform place = _places[_index];

        transform.position = Vector3.MoveTowards(transform.position, place.position, _speed * Time.deltaTime);

        if (transform.position == place.position)
            NextPlace();
    }

    private Vector3 NextPlace()
    {
        Vector3 nextPosition;

        if (_index++ == _places.Length)
            _index = 0;

        nextPosition = _places[_index].transform.position;
        transform.forward = nextPosition - transform.position;
        return nextPosition;
    }
}