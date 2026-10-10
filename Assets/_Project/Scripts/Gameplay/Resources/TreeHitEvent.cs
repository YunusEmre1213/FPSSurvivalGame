using Project.Core;

namespace Project.Gameplay.Resources
{
    public readonly struct TreeHitEvent : IGameEvent
    {
        public readonly int CurrentHits;
        public readonly int HitsRequired;

        public TreeHitEvent(int currentHits, int hitsRequired)
        {
            CurrentHits = currentHits;
            HitsRequired = hitsRequired;
        }
    }
}