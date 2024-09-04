using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mutating : MonoBehaviour
{
    private int _speed = 3;
    private int _speed2 = 1;

    private void Update()
    {
        transform.position += transform.forward * Time.deltaTime * _speed;
        transform.localScale += new Vector3(_speed2 * Time.deltaTime, _speed2 * Time.deltaTime, _speed2 * Time.deltaTime);
        transform.Rotate(0, _speed * Time.deltaTime * 5, 0);
    }
}