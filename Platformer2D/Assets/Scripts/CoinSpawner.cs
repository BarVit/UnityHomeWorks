using UnityEngine;

[RequireComponent(typeof(CoinPool))]
public class CoinSpawner : MonoBehaviour
{
    [SerializeField] Transform _coinsPlaces;

    private CoinPool _coinPool;
    private Transform[] _spawnPlaces;

    private void Start()
    {
        _coinPool = GetComponent<CoinPool>();
        _spawnPlaces = new Transform[_coinsPlaces.childCount];

        for (int i = 0; i < _spawnPlaces.Length; i++)
        {
            _spawnPlaces[i] = _coinsPlaces.GetChild(i);
        }

        foreach(Transform spawnPlace in _spawnPlaces)
        {
            Coin coin = _coinPool.GetCoin();

            coin.transform.position = spawnPlace.position;
        }
    }
}