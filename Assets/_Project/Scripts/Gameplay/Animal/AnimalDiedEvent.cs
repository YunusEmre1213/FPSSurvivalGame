using UnityEngine;
using Project.Core;

namespace Project.Gameplay.Animals
{
    public readonly struct AnimalDiedEvent : IGameEvent
    {
        public readonly Vector3 Position;

        public AnimalDiedEvent(Vector3 position)
        {
            Position = position;
        }
    }
}