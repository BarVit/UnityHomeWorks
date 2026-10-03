namespace WeaponContent
{
    public readonly struct BeamSounds
    {
        public BeamSounds(string shotPath, float shotVolume, string hitPath, float hitVolume,
            string worldHitPath, float worldHitVolume)
        {
            ShotPath = shotPath;
            ShotVolume = shotVolume;
            HitPath = hitPath;
            HitVolume = hitVolume;
            WorldHitPath = worldHitPath;
            WorldHitVolume = worldHitVolume;
        }

        public string ShotPath { get; }

        public float ShotVolume { get; }

        public string HitPath { get; }

        public float HitVolume { get; }

        public string WorldHitPath { get; }

        public float WorldHitVolume { get; }
    }
}
