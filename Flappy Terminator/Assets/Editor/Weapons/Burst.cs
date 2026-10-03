using UnityEngine;

namespace WeaponContent
{
    public readonly struct Burst
    {
        public Burst(string name, short count, Vector2 size, Vector2 lifetime, Vector2 speed,
            float coneAngle, Color from, Color to)
        {
            Name = name;
            Count = count;
            Size = size;
            Lifetime = lifetime;
            Speed = speed;
            ConeAngle = coneAngle;
            From = from;
            To = to;
        }

        public string Name { get; }

        public short Count { get; }

        public Vector2 Size { get; }

        public Vector2 Lifetime { get; }

        public Vector2 Speed { get; }

        public float ConeAngle { get; }

        public Color From { get; }

        public Color To { get; }
    }
}
