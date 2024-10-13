using UnityEngine;

public class Dwarf : Enemy
{
    public override void Init()
    {
        float scaleX = 2.5f;
        float scaleY = 1.5f;
        float scaleZ = 2.5f;

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        transform.position += new Vector3(0, scaleY / 2, 0);
    }
}