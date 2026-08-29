using System.IO;
using UnityEngine;
using Project.Core;
using Project.Data;
using Project.Gameplay.Weapons;

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
                data.inventoryEntries.Add(new InventoryEntry { partId = kvp.Key.partId, count = kvp.Value });
            }

            var keyItems = ServiceLocator.Instance.Get<IKeyItemService>();
            foreach (var keyId in keyItems.GetAllUnlockedKeys())
            {
                data.unlockedKeyIds.Add(keyId);
            }

            var weaponController = FindPlayerWeaponController();
            if (weaponController != null)
            {
                data.equippedWeapon.barrelPartId = weaponController.GetEquippedPart(WeaponPartType.Barrel)?.partId ?? "";
                data.equippedWeapon.magazinePartId = weaponController.GetEquippedPart(WeaponPartType.Magazine)?.partId ?? "";
                data.equippedWeapon.stockPartId = weaponController.GetEquippedPart(WeaponPartType.Stock)?.partId ?? "";
                data.equippedWeapon.sightPartId = weaponController.GetEquippedPart(WeaponPartType.Sight)?.partId ?? "";
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_savePath, json);
            Debug.Log($"[SaveService] Kaydedildi: {_savePath} ({data.inventoryEntries.Count} parca turu, {data.unlockedKeyIds.Count} anahtar)");
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
                    Debug.Log($"[SaveService] Parca yuklendi: {entry.partId} x{entry.count}");
                }
                else
                {
                    Debug.LogWarning($"[SaveService] Kayitli parca ID'si veritabaninda bulunamadi: {entry.partId}");
                }
            }

            var keyItems = ServiceLocator.Instance.Get<IKeyItemService>();
            foreach (var keyId in data.unlockedKeyIds)
            {
                keyItems.Unlock(keyId);
                Debug.Log($"[SaveService] Anahtar yuklendi: {keyId}");
            }

            var weaponController = FindPlayerWeaponController();
            if (weaponController != null && data.equippedWeapon != null)
            {
                var barrel = ResolvePart(data.equippedWeapon.barrelPartId);
                var magazine = ResolvePart(data.equippedWeapon.magazinePartId);
                var stock = ResolvePart(data.equippedWeapon.stockPartId);
                var sight = ResolvePart(data.equippedWeapon.sightPartId);

                weaponController.ApplyAssembly(barrel, magazine, stock, sight);
                Debug.Log("[SaveService] Silaha takili parcalar geri yuklendi.");
            }

            Debug.Log($"[SaveService] Yukleme tamamlandi: {_savePath}");
        }

        private WeaponPartData ResolvePart(string partId)
        {
            return string.IsNullOrEmpty(partId) ? null : _partDatabase.FindById(partId);
        }

        private WeaponController FindPlayerWeaponController()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            return playerObj != null ? playerObj.GetComponent<WeaponController>() : null;
        }
    }
}