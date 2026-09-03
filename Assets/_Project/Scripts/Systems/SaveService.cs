using System.IO;
using UnityEngine;
using Project.Core;
using Project.Data;
using Project.Gameplay.Weapons;

namespace Project.Systems
{
    public class SaveService : ISaveService
    {
        private readonly ItemDatabase _itemDatabase;
        private readonly string _savePath;

        public SaveService(ItemDatabase itemDatabase)
        {
            _itemDatabase = itemDatabase;
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

            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            for (int i = 0; i < inventory.SlotCount; i++)
            {
                var slot = inventory.GetSlot(i);
                if (slot.IsEmpty) continue;

                data.inventoryEntries.Add(new InventoryEntry
                {
                    itemId = slot.Item.itemId,
                    count = slot.Count
                });
            }

            var keyItems = ServiceLocator.Instance.Get<IKeyItemService>();
            foreach (var keyId in keyItems.GetAllUnlockedKeys())
            {
                data.unlockedKeyIds.Add(keyId);
            }

            var weaponController = FindPlayerWeaponController();
            if (weaponController != null)
            {
                data.equippedWeapon.barrelPartId = weaponController.GetEquippedPart(WeaponPartType.Barrel)?.itemId ?? "";
                data.equippedWeapon.magazinePartId = weaponController.GetEquippedPart(WeaponPartType.Magazine)?.itemId ?? "";
                data.equippedWeapon.stockPartId = weaponController.GetEquippedPart(WeaponPartType.Stock)?.itemId ?? "";
                data.equippedWeapon.sightPartId = weaponController.GetEquippedPart(WeaponPartType.Sight)?.itemId ?? "";
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_savePath, json);
            Debug.Log($"[SaveService] Kaydedildi: {_savePath} ({data.inventoryEntries.Count} esya turu, {data.unlockedKeyIds.Count} anahtar)");
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

            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            foreach (var entry in data.inventoryEntries)
            {
                var item = _itemDatabase.FindById(entry.itemId);
                if (item != null)
                {
                    inventory.AddItem(item, entry.count);
                    Debug.Log($"[SaveService] Yuklendi: {entry.itemId} x{entry.count}");
                }
                else
                {
                    Debug.LogWarning($"[SaveService] Kayitli esya ID'si veritabaninda bulunamadi: {entry.itemId}");
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

        private WeaponPartData ResolvePart(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            return _itemDatabase.FindById(itemId) as WeaponPartData;
        }

        private WeaponController FindPlayerWeaponController()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            return playerObj != null ? playerObj.GetComponent<WeaponController>() : null;
        }
    }
}