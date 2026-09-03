using System.Collections.Generic;
using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "MobilFPS/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemData> allItems = new List<ItemData>();

        public ItemData FindById(string itemId)
        {
            foreach (var item in allItems)
            {
                if (item != null && item.itemId == itemId)
                {
                    return item;
                }
            }
            return null;
        }
    }
}