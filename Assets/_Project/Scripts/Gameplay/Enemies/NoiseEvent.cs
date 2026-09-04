using UnityEngine;
using Project.Core;

namespace Project.Gameplay.Enemies
{
    public readonly struct NoiseEvent : IGameEvent
    {
        public readonly Vector3 Position;
        public readonly float Radius;

        public NoiseEvent(Vector3 position, float radius)
        {
            Position = position;
            Radius = radius;
        }
    }
}