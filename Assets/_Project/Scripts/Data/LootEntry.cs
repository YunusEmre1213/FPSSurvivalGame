using System;
using UnityEngine;

namespace Project.Data
{
    [Serializable]
    public class LootEntry
    {
        public ItemData item;
        public float weight = 1f;
        public int minAmount = 1;
        public int maxAmount = 1;
    }
}