using UnityEngine;

namespace Project.Core
{
    public readonly struct EnemyKilledEvent : IGameEvent
    {
        public readonly string EnemyType;
        public readonly Vector3 Position;

        public EnemyKilledEvent(string enemyType, Vector3 position)
        {
            EnemyType = enemyType;
            Position = position;
        }
    }
}