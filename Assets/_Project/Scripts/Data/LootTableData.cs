using System.Collections.Generic;
using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(fileName = "NewLootTable", menuName = "MobilFPS/Loot Table")]
    public class LootTableData : ScriptableObject
    {
        public List<LootEntry> entries = new List<LootEntry>();

        [Tooltip("Konteynerden kac AYRI cekilis yapilacagini belirler - ayni esya birden fazla kez cikabilir (ornegin 2 cekilis 'Yemek' getirebilir).")]
        [Range(1, 10)]
        public int rollCount = 1;

        public List<(ItemData item, int amount)> Roll()
        {
            var results = new List<(ItemData, int)>();
            if (entries == null || entries.Count == 0) return results;

            float totalWeight = 0f;
            foreach (var entry in entries)
            {
                totalWeight += entry.weight;
            }

            if (totalWeight <= 0f) return results;

            for (int i = 0; i < rollCount; i++)
            {
                float roll = Random.Range(0f, totalWeight);
                float cumulative = 0f;

                foreach (var entry in entries)
                {
                    cumulative += entry.weight;
                    if (roll <= cumulative)
                    {
                        int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                        results.Add((entry.item, amount));
                        break;
                    }
                }
            }

            return results;
        }
    }
}