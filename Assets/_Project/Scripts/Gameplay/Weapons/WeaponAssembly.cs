using System.Collections.Generic;
using UnityEngine;
using Project.Data;

namespace Project.Gameplay.Weapons
{
    /// Çalýþma zamanýnda bir silahýn taban verisini ve o an takýlý parçalarýný tutan
    /// nihai statlarý hesaplayan sýnýf
    public class WeaponAssembly
    {
        private readonly WeaponBaseData _baseData;
        private readonly Dictionary<WeaponPartType, WeaponPartData> _equippedParts = new Dictionary<WeaponPartType, WeaponPartData>();

        public WeaponAssembly(WeaponBaseData baseData)
        {
            _baseData = baseData;
        }

        public void EquipPart(WeaponPartData part)
        {
            if (part == null) return;
            _equippedParts[part.partType] = part;
        }

        public void RemovePart(WeaponPartType slot)
        {
            _equippedParts.Remove(slot);
        }

        public WeaponPartData GetEquippedPart(WeaponPartType slot)
        {
            return _equippedParts.TryGetValue(slot, out var part) ? part : null;
        }

        public WeaponStats CalculateStats()
        {
            float damage = _baseData.baseDamage;
            float accuracy = _baseData.baseAccuracy;
            float fireRate = _baseData.baseFireRate;
            float recoil = _baseData.baseRecoil;
            float durability = _baseData.baseDurability;
            int ammoCapacity = _baseData.baseAmmoCapacity;
            float malfunctionChance = 0f;

            foreach (var part in _equippedParts.Values)
            {
                damage += part.damageModifier;
                accuracy += part.accuracyModifier;
                fireRate += part.fireRateModifier;
                recoil += part.recoilModifier;
                durability += part.durabilityModifier;
                ammoCapacity += part.ammoCapacityModifier;
                malfunctionChance += part.malfunctionChance;
            }

            accuracy = Mathf.Clamp01(accuracy);
            malfunctionChance = Mathf.Clamp01(malfunctionChance);
            fireRate = Mathf.Max(0.1f, fireRate);
            damage = Mathf.Max(0f, damage);

            return new WeaponStats(damage, accuracy, fireRate, recoil, durability, ammoCapacity, malfunctionChance);
        }
    }
}