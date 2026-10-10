public class RunState
{
    private const int FirstLevel = 1;

    public int Level { get; private set; } = FirstLevel;

    public Reward PendingReward { get; private set; }

    public Reward BankedReward { get; private set; }

    public void Restart()
    {
        Level = FirstLevel;
        PendingReward = default;
        BankedReward = default;
    }

    public void RestartRun()
    {
        Level = FirstLevel;
        PendingReward = default;
    }

    public void AddReward(Reward reward)
    {
        PendingReward += reward;
    }

    public void DropPending()
    {
        PendingReward = default;
    }

    public void BankReward()
    {
        BankedReward += PendingReward;
        PendingReward = default;
    }

    public void AdvanceLevel()
    {
        Level++;
    }
}
