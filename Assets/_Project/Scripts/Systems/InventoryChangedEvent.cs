using Project.Core;
using Project.Data;

namespace Project.Systems
{
    public readonly struct InventoryChangedEvent : IGameEvent
    {
        public readonly WeaponPartData Part;
        public readonly int NewCount;

        public InventoryChangedEvent(WeaponPartData part, int newCount)
        {
            Part = part;
            NewCount = newCount;
        }
    }
}