using Project.Core;
using Project.Data;

namespace Project.Systems
{
    public interface IItemInventoryService : IGameService
    {
        int SlotCount { get; }
        InventorySlot GetSlot(int index);
        int AddItem(ItemData item, int amount);
        int RemoveItem(ItemData item, int amount);
        int GetItemCount(ItemData item);
        void ClearAll();
    }
}