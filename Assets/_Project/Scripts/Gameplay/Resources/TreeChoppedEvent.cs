using UnityEngine;
using Project.Core;

namespace Project.Gameplay.Resources
{
    public readonly struct TreeChoppedEvent : IGameEvent
    {
        public readonly Vector3 Position;

        public TreeChoppedEvent(Vector3 position)
        {
            Position = position;
        }
    }
}