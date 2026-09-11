using UnityEngine;

public class EnemyDeadBody : MonoBehaviour
{
    public void End()
    {
        Destroy(gameObject, 0.4f);
    }
}