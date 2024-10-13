using UnityEngine;

public class Orc : Enemy
{
    public override void SetParameters()
    {
        float scaleX = 1.5f;
        float scaleY = 2.5f;
        float scaleZ = 1.5f;

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        GetComponent<Renderer>().material.color = Color.red;
    }
}