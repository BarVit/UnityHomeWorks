using UnityEngine;

public class Human : Enemy
{
    public override void SetParameters()
    {
        float scaleX = 1f;
        float scaleY = 2f;
        float scaleZ = 1f;

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        GetComponent<Renderer>().material.color = Color.blue;
    }
}