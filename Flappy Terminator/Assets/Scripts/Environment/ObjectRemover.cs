using UnityEngine;

public class ObjectRemover : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        IRemoveable removeable = other.GetComponentInParent<IRemoveable>();

        removeable?.Remove();
    }
}
