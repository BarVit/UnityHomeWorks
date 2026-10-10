using System;

public class ShotTally
{
    public event Action Fired;
    public event Action Hit;

    public void CountFired()
    {
        Fired?.Invoke();
    }

    public void CountHit()
    {
        Hit?.Invoke();
    }
}
