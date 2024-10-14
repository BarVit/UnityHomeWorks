using UnityEngine;

public class Goblin : Enemy
{
    public override void Init()
    {
        float scaleX = 0.6f;
        float scaleY = 1f;
        float scaleZ = 0.6f;
        float verticalCorrection = 2f;

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        transform.position += new Vector3(0, scaleY / verticalCorrection, 0);
    }
}