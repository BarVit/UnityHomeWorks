namespace WeaponContent
{
    public readonly struct BeamSounds
    {
        public BeamSounds(string shotPath, float shotVolume, string hitPath, float hitVolume,
            string worldHitPath, float worldHitVolume, string loopPath, float loopVolume,
            string hitLoopPath, float hitLoopVolume)
        {
            ShotPath = shotPath;
            ShotVolume = shotVolume;
            HitPath = hitPath;
            HitVolume = hitVolume;
            WorldHitPath = worldHitPath;
            WorldHitVolume = worldHitVolume;
            LoopPath = loopPath;
            LoopVolume = loopVolume;
            HitLoopPath = hitLoopPath;
            HitLoopVolume = hitLoopVolume;
        }

        public string ShotPath { get; }

        public float ShotVolume { get; }

        public string HitPath { get; }

        public float HitVolume { get; }

        public string WorldHitPath { get; }

        public float WorldHitVolume { get; }

        public string LoopPath { get; }

        public float LoopVolume { get; }

        public string HitLoopPath { get; }

        public float HitLoopVolume { get; }
    }
}
