public class ArcGrab
{
    public ArcGrab(float grabbedAt)
    {
        GrabbedAt = grabbedAt;
    }

    public float GrabbedAt { get; }

    public float Pending { get; private set; }

    public int Barrel { get; private set; }

    public bool IsSeen { get; private set; }

    public void See()
    {
        IsSeen = true;
    }

    public void Forget()
    {
        IsSeen = false;
    }

    public void Delay(float damage, int barrel)
    {
        Pending += damage;
        Barrel = barrel;
    }

    public float TakePending()
    {
        float pending = Pending;

        Pending = 0f;

        return pending;
    }
}
