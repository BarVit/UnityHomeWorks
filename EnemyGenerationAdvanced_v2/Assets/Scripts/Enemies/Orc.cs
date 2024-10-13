using UnityEngine;

public class Orc : Enemy
{
    public override void Init()
    {
        float scaleX = 2f;
        float scaleY = 2.5f;
        float scaleZ = 2f;

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        transform.position += new Vector3(0, scaleY / 2, 0);
    }
}