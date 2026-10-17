namespace TableImport
{
    public readonly struct ShieldLevel
    {
        public ShieldLevel(int capacity, Reward reward)
        {
            Capacity = capacity;
            Reward = reward;
        }

        public int Capacity { get; }

        public Reward Reward { get; }
    }
}
