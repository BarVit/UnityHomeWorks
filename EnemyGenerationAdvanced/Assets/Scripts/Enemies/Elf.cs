using UnityEngine;

public class Elf : Enemy
{
    public override void SetParameters()
    {
        float scaleX = 0.75f;
        float scaleY = 2.5f;
        float scaleZ = 0.75f;

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        GetComponent<Renderer>().material.color = Color.yellow;
    }
}