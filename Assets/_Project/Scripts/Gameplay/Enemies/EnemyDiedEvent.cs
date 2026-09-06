using UnityEngine;
using Project.Core;

namespace Project.Gameplay.Enemies
{
    public readonly struct EnemyDiedEvent : IGameEvent
    {
        public readonly Vector3 Position;

        public EnemyDiedEvent(Vector3 position)
        {
            Position = position;
        }
    }
}