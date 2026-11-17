using UnityEngine;

namespace Project.Core
{
    public readonly struct AnimalDiedEvent : IGameEvent
    {
        public readonly Vector3 Position;
        public readonly int MeatAmount;
        public readonly string AnimalType;

        public AnimalDiedEvent(Vector3 position, int meatAmount, string animalType)
        {
            Position = position;
            MeatAmount = meatAmount;
            AnimalType = animalType;
        }
    }
}