using System;
using System.Collections.Generic;

namespace Project.Systems
{
    [Serializable]
    public class SaveData
    {
        public List<InventoryEntry> inventoryEntries = new List<InventoryEntry>();
    }

    [Serializable]
    public class InventoryEntry
    {
        public string partId;
        public int count;
    }
}