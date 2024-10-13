using UnityEngine;

public class Goblin : Enemy
{
    public override void SetParameters()
    {
        float scaleX = 0.5f;
        float scaleY = 1f;
        float scaleZ = 0.5f;

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        GetComponent<Renderer>().material.color = Color.green;
    }
}