using System.Collections.Generic;
using UnityEngine;

public class SpriteSkin : ScriptableObject
{
    [SerializeField] private Sprite[] _from;
    [SerializeField] private Sprite[] _to;

    private Dictionary<Sprite, Sprite> _map;

    public Sprite Get(Sprite sprite)
    {
        if (sprite == null)
            return null;

        if (_map == null)
            Build();

        return _map.TryGetValue(sprite, out Sprite skinned) ? skinned : sprite;
    }

    private void Build()
    {
        _map = new Dictionary<Sprite, Sprite>();

        for (int i = 0; i < _from.Length && i < _to.Length; i++)
            _map[_from[i]] = _to[i];
    }

#if UNITY_EDITOR
    public void Fill(Sprite[] from, Sprite[] to)
    {
        _from = from;
        _to = to;
        _map = null;
    }
#endif
}
