using System;
using System.Collections.Generic;

namespace Project.Systems
{
    [Serializable]
    public class SaveData
    {
        public List<InventoryEntry> inventoryEntries = new List<InventoryEntry>();
        public List<string> unlockedKeyIds = new List<string>();
        public EquippedWeaponEntry equippedWeapon = new EquippedWeaponEntry();
    }

    [Serializable]
    public class InventoryEntry
    {
        public string partId;
        public int count;
    }

    [Serializable]
    public class EquippedWeaponEntry
    {
        public string barrelPartId = "";
        public string magazinePartId = "";
        public string stockPartId = "";
        public string sightPartId = "";
    }
}