public readonly struct Reward
{
    public Reward(int xp, int money)
    {
        Xp = xp;
        Money = money;
    }

    public int Xp { get; }

    public int Money { get; }

    public bool IsEmpty => Xp == 0 && Money == 0;

    public static Reward operator +(Reward left, Reward right)
    {
        return new Reward(left.Xp + right.Xp, left.Money + right.Money);
    }
}
