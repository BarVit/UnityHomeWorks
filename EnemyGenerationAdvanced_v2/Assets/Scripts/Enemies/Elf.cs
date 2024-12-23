using UnityEngine;

public class Elf : Enemy
{
    public override void Init()
    {
        float scaleX = 1f;
        float scaleY = 2.5f;
        float scaleZ = 1f;
        float verticalCorrection = 2f;

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        transform.position += new Vector3(0, scaleY / verticalCorrection, 0);
    }
}