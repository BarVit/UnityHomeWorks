namespace TableImport
{
    public readonly struct ShieldLevel
    {
        public ShieldLevel(int level, int capacity, Reward reward)
        {
            Level = level;
            Capacity = capacity;
            Reward = reward;
        }

        public int Level { get; }

        public int Capacity { get; }

        public Reward Reward { get; }
    }
}
