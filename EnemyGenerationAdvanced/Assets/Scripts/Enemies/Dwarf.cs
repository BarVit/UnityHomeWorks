using UnityEngine;

public class Dwarf : Enemy
{
    public override void SetParameters()
    {
        float scaleX = 2f;
        float scaleY = 1.5f;
        float scaleZ = 2f;

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        GetComponent<Renderer>().material.color = Color.gray;
    }
}