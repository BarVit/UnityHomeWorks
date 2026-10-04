using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Movement/Forward")]
public class ForwardProfile : MovementProfile
{
    public override EnemyMovement Create(Rigidbody2D body, Camera camera)
    {
        return new ForwardMovement(body, camera);
    }
}
