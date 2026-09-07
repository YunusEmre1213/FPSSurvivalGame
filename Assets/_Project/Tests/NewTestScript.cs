using NUnit.Framework;
using UnityEngine;
using Project.Data;
using Project.Gameplay.Weapons;

namespace Project.Tests.EditMode
{
    public class WeaponAssemblyTests
    {
        private WeaponBaseData _baseWeapon;

        [SetUp]
        public void SetUp()
        {
            _baseWeapon = ScriptableObject.CreateInstance<WeaponBaseData>();
            _baseWeapon.baseDamage = 10f;
            _baseWeapon.baseAccuracy = 0.7f;
            _baseWeapon.baseFireRate = 2f;
            _baseWeapon.baseRecoil = 0.2f;
            _baseWeapon.baseDurability = 100f;
            _baseWeapon.baseAmmoCapacity = 10;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_baseWeapon);
        }

        [Test]
        public void CalculateStats_NoParts_ReturnsBaseStats()
        {
            var assembly = new WeaponAssembly(_baseWeapon);
            var stats = assembly.CalculateStats();

            Assert.AreEqual(10f, stats.Damage);
            Assert.AreEqual(0.7f, stats.Accuracy, 0.0001f);
            Assert.AreEqual(10, stats.AmmoCapacity);
            Assert.AreEqual(0f, stats.MalfunctionChance);
        }

        [Test]
        public void CalculateStats_WithBarrelPart_AddsModifiersToBase()
        {
            var barrel = ScriptableObject.CreateInstance<WeaponPartData>();
            barrel.partType = WeaponPartType.Barrel;
            barrel.damageModifier = 5f;
            barrel.accuracyModifier = 0.05f;

            var assembly = new WeaponAssembly(_baseWeapon);
            assembly.EquipPart(barrel);
            var stats = assembly.CalculateStats();

            Assert.AreEqual(15f, stats.Damage);
            Assert.AreEqual(0.75f, stats.Accuracy, 0.0001f);

            Object.DestroyImmediate(barrel);
        }

        [Test]
        public void CalculateStats_MultipleCorruptedParts_AccumulatesMalfunctionChance()
        {
            var sight = ScriptableObject.CreateInstance<WeaponPartData>();
            sight.partType = WeaponPartType.Sight;
            sight.malfunctionChance = 0.08f;

            var stock = ScriptableObject.CreateInstance<WeaponPartData>();
            stock.partType = WeaponPartType.Stock;
            stock.malfunctionChance = 0.05f;

            var assembly = new WeaponAssembly(_baseWeapon);
            assembly.EquipPart(sight);
            assembly.EquipPart(stock);
            var stats = assembly.CalculateStats();

            Assert.AreEqual(0.13f, stats.MalfunctionChance, 0.0001f);

            Object.DestroyImmediate(sight);
            Object.DestroyImmediate(stock);
        }

        [Test]
        public void CalculateStats_AccuracyAboveOne_IsClampedToOne()
        {
            _baseWeapon.baseAccuracy = 0.95f;
            var sight = ScriptableObject.CreateInstance<WeaponPartData>();
            sight.partType = WeaponPartType.Sight;
            sight.accuracyModifier = 0.5f; 

            var assembly = new WeaponAssembly(_baseWeapon);
            assembly.EquipPart(sight);
            var stats = assembly.CalculateStats();

            Assert.AreEqual(1f, stats.Accuracy, 0.0001f);

            Object.DestroyImmediate(sight);
        }

        [Test]
        public void CalculateStats_DamageBelowZero_IsClampedToZero()
        {
            var barrel = ScriptableObject.CreateInstance<WeaponPartData>();
            barrel.partType = WeaponPartType.Barrel;
            barrel.damageModifier = -100f; 

            var assembly = new WeaponAssembly(_baseWeapon);
            assembly.EquipPart(barrel);
            var stats = assembly.CalculateStats();

            Assert.AreEqual(0f, stats.Damage);

            Object.DestroyImmediate(barrel);
        }

        [Test]
        public void EquipPart_SameSlotTwice_ReplacesOldPart()
        {
            var barrel1 = ScriptableObject.CreateInstance<WeaponPartData>();
            barrel1.partType = WeaponPartType.Barrel;
            barrel1.damageModifier = 5f;

            var barrel2 = ScriptableObject.CreateInstance<WeaponPartData>();
            barrel2.partType = WeaponPartType.Barrel;
            barrel2.damageModifier = 12f;

            var assembly = new WeaponAssembly(_baseWeapon);
            assembly.EquipPart(barrel1);
            assembly.EquipPart(barrel2); 

            var stats = assembly.CalculateStats();

            Assert.AreEqual(22f, stats.Damage); 

            Object.DestroyImmediate(barrel1);
            Object.DestroyImmediate(barrel2);
        }
    }
}