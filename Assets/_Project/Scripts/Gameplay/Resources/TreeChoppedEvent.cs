using UnityEngine;

namespace Project.Core
{
    public readonly struct TreeChoppedEvent : IGameEvent
    {
        public readonly Vector3 Position;
        public readonly int WoodAmount;

        public TreeChoppedEvent(Vector3 position, int woodAmount)
        {
            Position = position;
            WoodAmount = woodAmount;
        }
    }
}