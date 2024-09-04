using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scaling : MonoBehaviour
{
    private int _speed = 3;
    private int _scaleChanger = -1;

    private void Update()
    {
        if (transform.localScale.x > 3 && _speed > 0)
            _speed *= _scaleChanger;
        else if (transform.localScale.x < 0.5 && _speed < 0)
            _speed *= _scaleChanger;

        float s = _speed * Time.deltaTime;

        transform.localScale += new Vector3(s, s, s);
    }
}