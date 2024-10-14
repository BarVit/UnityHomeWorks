using UnityEngine;

public class Human : Enemy
{
    public override void Init()
    {
        float scaleX = 1.25f;
        float scaleY = 2f;
        float scaleZ = 1.25f;
        float verticalCorrection = 2f;

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        transform.position += new Vector3(0, scaleY / verticalCorrection, 0);
    }
}