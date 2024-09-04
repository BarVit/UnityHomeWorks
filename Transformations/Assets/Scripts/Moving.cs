using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moving : MonoBehaviour
{
    private int _speed = 3;
    private int _directionChanger = -1;

    private void Update()
    {
        if (transform.position.z > 3 && _speed > 0)
            _speed *= _directionChanger;
        else if (transform.position.z < -3 && _speed < 0)
            _speed *= _directionChanger;

        transform.position += new Vector3(0, 0, _speed * Time.deltaTime);
    }
}