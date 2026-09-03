using Project.Data;

namespace Project.Systems
{
    public class InventorySlot
    {
        public ItemData Item { get; private set; }
        public int Count { get; private set; }

        public bool IsEmpty => Item == null || Count <= 0;

        public int TryAdd(ItemData item, int amount)
        {
            if (amount <= 0) return 0;

            if (IsEmpty)
            {
                Item = item;
                int fits = UnityEngine.Mathf.Min(amount, item.maxStackSize);
                Count = fits;
                return amount - fits;
            }

            if (Item != item) return amount; 

            int room = Item.maxStackSize - Count;
            int added = UnityEngine.Mathf.Min(amount, room);
            Count += added;
            return amount - added;
        }
        public int TryRemove(int amount)
        {
            if (IsEmpty || amount <= 0) return 0;

            int removed = UnityEngine.Mathf.Min(amount, Count);
            Count -= removed;
            if (Count <= 0)
            {
                Clear();
            }
            return removed;
        }

        public void Clear()
        {
            Item = null;
            Count = 0;
        }
    }
}