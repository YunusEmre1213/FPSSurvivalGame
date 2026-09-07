using NUnit.Framework;
using UnityEngine;
using Project.Data;
using Project.Systems;

namespace Project.Tests.EditMode
{
    public class ItemInventoryServiceTests
    {
        private ResourceData _itemA;
        private ResourceData _itemB;

        [SetUp]
        public void SetUp()
        {
            _itemA = ScriptableObject.CreateInstance<ResourceData>();
            _itemA.itemId = "ItemA";
            _itemA.maxStackSize = 10;

            _itemB = ScriptableObject.CreateInstance<ResourceData>();
            _itemB.itemId = "ItemB";
            _itemB.maxStackSize = 10;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_itemA);
            Object.DestroyImmediate(_itemB);
        }

        [Test]
        public void AddItem_ToFreshInventory_FillsFirstEmptySlot()
        {
            var inventory = new ItemInventoryService(4);
            int leftover = inventory.AddItem(_itemA, 5);

            Assert.AreEqual(0, leftover);
            Assert.AreEqual(5, inventory.GetItemCount(_itemA));
        }

        [Test]
        public void AddItem_SameItemTwice_StacksIntoSameSlotFirst()
        {
            var inventory = new ItemInventoryService(4);
            inventory.AddItem(_itemA, 3);
            inventory.AddItem(_itemA, 4);

            Assert.AreEqual(7, inventory.GetItemCount(_itemA));
            Assert.AreEqual(7, inventory.GetSlot(0).Count); 
            Assert.IsTrue(inventory.GetSlot(1).IsEmpty);
        }

        [Test]
        public void AddItem_WhenInventoryFull_ReturnsLeftover()
        {
            var inventory = new ItemInventoryService(1); 
            inventory.AddItem(_itemA, 10); 
            int leftover = inventory.AddItem(_itemA, 5); 

            Assert.AreEqual(5, leftover);
            Assert.AreEqual(10, inventory.GetItemCount(_itemA));
        }

        [Test]
        public void RemoveItem_AcrossMultipleSlots_RemovesCorrectTotal()
        {
            var inventory = new ItemInventoryService(4);
            inventory.AddItem(_itemA, 10); 
            inventory.AddItem(_itemA, 5);  

            int removed = inventory.RemoveItem(_itemA, 12);

            Assert.AreEqual(12, removed);
            Assert.AreEqual(3, inventory.GetItemCount(_itemA));
        }

        [Test]
        public void GetItemCount_DifferentItems_AreCountedSeparately()
        {
            var inventory = new ItemInventoryService(4);
            inventory.AddItem(_itemA, 6);
            inventory.AddItem(_itemB, 3);

            Assert.AreEqual(6, inventory.GetItemCount(_itemA));
            Assert.AreEqual(3, inventory.GetItemCount(_itemB));
        }

        [Test]
        public void ClearAll_EmptiesEverySlot()
        {
            var inventory = new ItemInventoryService(4);
            inventory.AddItem(_itemA, 5);
            inventory.AddItem(_itemB, 3);

            inventory.ClearAll();

            Assert.AreEqual(0, inventory.GetItemCount(_itemA));
            Assert.AreEqual(0, inventory.GetItemCount(_itemB));
        }
    }
}