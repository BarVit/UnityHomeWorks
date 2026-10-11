using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelBook
{
    [SerializeField] private List<string> _keys = new List<string>();
    [SerializeField] private List<int> _levels = new List<int>();

    public int Get(string key)
    {
        int index = _keys.IndexOf(key);

        return index < 0 ? 0 : _levels[index];
    }

    public void Raise(string key)
    {
        int index = _keys.IndexOf(key);

        if (index < 0)
        {
            _keys.Add(key);
            _levels.Add(1);

            return;
        }

        _levels[index]++;
    }
}
