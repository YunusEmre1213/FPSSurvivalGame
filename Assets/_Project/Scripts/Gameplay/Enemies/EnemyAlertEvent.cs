using UnityEngine;
using Project.Core;

namespace Project.Gameplay.Enemies
{
    public readonly struct EnemyAlertEvent : IGameEvent
    {
        public readonly Vector3 AlertPosition;
        public readonly GameObject Source;

        public EnemyAlertEvent(Vector3 alertPosition, GameObject source)
        {
            AlertPosition = alertPosition;
            Source = source;
        }
    }
}