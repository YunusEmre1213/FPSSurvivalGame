using Project.Core;
using Project.Data;

namespace Project.Systems
{
    public class ItemInventoryService : IItemInventoryService
    {
        private readonly InventorySlot[] _slots;

        public ItemInventoryService(int slotCount = 24)
        {
            _slots = new InventorySlot[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                _slots[i] = new InventorySlot();
            }
        }

        public int SlotCount => _slots.Length;

        public void Initialize()
        {
        }

        public void Shutdown()
        {
        }

        public InventorySlot GetSlot(int index)
        {
            return _slots[index];
        }

        public int AddItem(ItemData item, int amount)
        {
            if (item == null || amount <= 0) return amount;

            int remaining = amount;

            foreach (var slot in _slots)
            {
                if (remaining <= 0) break;
                if (!slot.IsEmpty && slot.Item == item)
                {
                    remaining = slot.TryAdd(item, remaining);
                }
            }
            foreach (var slot in _slots)
            {
                if (remaining <= 0) break;
                if (slot.IsEmpty)
                {
                    remaining = slot.TryAdd(item, remaining);
                }
            }

            if (remaining < amount)
            {
                EventBus.Publish(new InventorySlotChangedEvent());
            }

            return remaining; 
        }

        public int RemoveItem(ItemData item, int amount)
        {
            if (item == null || amount <= 0) return 0;

            int remaining = amount;
            foreach (var slot in _slots)
            {
                if (remaining <= 0) break;
                if (!slot.IsEmpty && slot.Item == item)
                {
                    remaining -= slot.TryRemove(remaining);
                }
            }

            int removed = amount - remaining;
            if (removed > 0)
            {
                EventBus.Publish(new InventorySlotChangedEvent());
            }
            return removed;
        }

        public int GetItemCount(ItemData item)
        {
            int total = 0;
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.Item == item)
                {
                    total += slot.Count;
                }
            }
            return total;
        }

        public void ClearAll()
        {
            foreach (var slot in _slots)
            {
                slot.Clear();
            }
            EventBus.Publish(new InventorySlotChangedEvent());
        }
    }
}