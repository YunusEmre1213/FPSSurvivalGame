using System.IO;
using UnityEngine;
using Project.Core;
using Project.Data;

namespace Project.Systems
{
    public class SaveService : ISaveService
    {
        private readonly WeaponPartDatabase _partDatabase;
        private readonly string _savePath;

        public SaveService(WeaponPartDatabase partDatabase)
        {
            _partDatabase = partDatabase;
            _savePath = Path.Combine(Application.persistentDataPath, "save.json");
        }

        public void Initialize()
        {
        }

        public void Shutdown()
        {
        }

        public void Save()
        {
            var data = new SaveData();
            var inventory = ServiceLocator.Instance.Get<IInventoryService>();

            foreach (var kvp in inventory.GetAllParts())
            {
                if (kvp.Key == null) continue;

                data.inventoryEntries.Add(new InventoryEntry
                {
                    partId = kvp.Key.partId,
                    count = kvp.Value
                });
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_savePath, json);
            Debug.Log($"[SaveService] Kaydedildi: {_savePath} ({data.inventoryEntries.Count} parca turu)");
        }

        public void Load()
        {
            if (!File.Exists(_savePath))
            {
                Debug.Log("[SaveService] Kayit dosyasi yok, yuklenecek bir sey bulunamadi.");
                return;
            }

            string json = File.ReadAllText(_savePath);
            var data = JsonUtility.FromJson<SaveData>(json);
            var inventory = ServiceLocator.Instance.Get<IInventoryService>();

            foreach (var entry in data.inventoryEntries)
            {
                var part = _partDatabase.FindById(entry.partId);
                if (part != null)
                {
                    inventory.AddPart(part, entry.count);
                    Debug.Log($"[SaveService] Yuklendi: {entry.partId} x{entry.count}");
                }
                else
                {
                    Debug.LogWarning($"[SaveService] Kayitli parca ID'si veritabaninda bulunamadi: {entry.partId}");
                }
            }

            Debug.Log($"[SaveService] Yukleme tamamlandi: {_savePath}");
        }
    }
}