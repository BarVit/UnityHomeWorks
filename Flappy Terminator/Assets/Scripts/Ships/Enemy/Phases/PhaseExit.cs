public class PhaseExit
{
    public virtual void Tick(float deltaTime)
    {
    }

    public virtual void CountShot()
    {
    }

    public virtual bool IsDone(float healthShare)
    {
        return false;
    }
}
