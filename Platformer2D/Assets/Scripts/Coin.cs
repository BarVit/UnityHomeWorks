using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Coin : MonoBehaviour
{
    private string _animationName = "Idle";

    private void Start()
    {
        GetComponent<Animator>().Play(_animationName, 0, 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.GetComponent<Player>() != null)
        {
            collision.transform.GetComponent<Player>().TakeCoin();
            Destroy(gameObject);
        }
    }
}