using UnityEngine;

public class Target2 : Target
{
    private Vector3 _rotatingCenter = new Vector3(35, 0.5f, -35);
    private float _speed = 40;

    public override void Update()
    {
        transform.RotateAround(_rotatingCenter, Vector3.up, _speed * Time.deltaTime);
    }
}