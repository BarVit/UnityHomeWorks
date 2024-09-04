using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotating : MonoBehaviour
{
    private int _speed = 100;

    private void Update()
    {
        transform.Rotate(0, _speed * Time.deltaTime, 0);
    }
}