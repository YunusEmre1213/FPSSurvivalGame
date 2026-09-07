using NUnit.Framework;
using UnityEngine;
using Project.Data;
using Project.Systems;

namespace Project.Tests.EditMode
{
    public class InventorySlotTests
    {
        private ResourceData _testItem;

        [SetUp]
        public void SetUp()
        {
            _testItem = ScriptableObject.CreateInstance<ResourceData>();
            _testItem.itemId = "TestItem";
            _testItem.itemName = "Test Item";
            _testItem.maxStackSize = 10;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testItem);
        }

        [Test]
        public void TryAdd_ToEmptySlot_FillsCorrectly()
        {
            var slot = new InventorySlot();
            int leftover = slot.TryAdd(_testItem, 5);

            Assert.AreEqual(0, leftover);
            Assert.AreEqual(5, slot.Count);
            Assert.AreEqual(_testItem, slot.Item);
            Assert.IsFalse(slot.IsEmpty);
        }

        [Test]
        public void TryAdd_ExceedsMaxStack_ReturnsLeftover()
        {
            var slot = new InventorySlot();
            int leftover = slot.TryAdd(_testItem, 15); 

            Assert.AreEqual(5, leftover);
            Assert.AreEqual(10, slot.Count);
        }

        [Test]
        public void TryAdd_DifferentItemToOccupiedSlot_ReturnsFullAmountUnchanged()
        {
            var otherItem = ScriptableObject.CreateInstance<ResourceData>();
            otherItem.maxStackSize = 10;

            var slot = new InventorySlot();
            slot.TryAdd(_testItem, 5);
            int leftover = slot.TryAdd(otherItem, 3);

            Assert.AreEqual(3, leftover); 
            Assert.AreEqual(5, slot.Count);
            Assert.AreEqual(_testItem, slot.Item);

            Object.DestroyImmediate(otherItem);
        }

        [Test]
        public void TryRemove_PartialAmount_LeavesRemainder()
        {
            var slot = new InventorySlot();
            slot.TryAdd(_testItem, 8);
            int removed = slot.TryRemove(3);

            Assert.AreEqual(3, removed);
            Assert.AreEqual(5, slot.Count);
            Assert.IsFalse(slot.IsEmpty);
        }

        [Test]
        public void TryRemove_FullAmount_ClearsSlot()
        {
            var slot = new InventorySlot();
            slot.TryAdd(_testItem, 5);
            slot.TryRemove(5);

            Assert.IsTrue(slot.IsEmpty);
            Assert.IsNull(slot.Item);
        }

        [Test]
        public void TryRemove_MoreThanAvailable_RemovesOnlyWhatExists()
        {
            var slot = new InventorySlot();
            slot.TryAdd(_testItem, 4);
            int removed = slot.TryRemove(10);

            Assert.AreEqual(4, removed); 
            Assert.IsTrue(slot.IsEmpty);
        }
    }
}