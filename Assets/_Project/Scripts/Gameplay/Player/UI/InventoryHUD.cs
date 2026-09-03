using System.Text;
using UnityEngine;
using TMPro;
using Project.Core;
using Project.Systems;

namespace Project.UI
{
    public class InventoryHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text inventoryText;

        private void OnEnable()
        {
            EventBus.Subscribe<InventorySlotChangedEvent>(OnInventoryChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InventorySlotChangedEvent>(OnInventoryChanged);
        }

        private void Start()
        {
            Refresh();
        }

        private void OnInventoryChanged(InventorySlotChangedEvent evt)
        {
            Refresh();
        }

        private void Refresh()
        {
            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            var sb = new StringBuilder();

            for (int i = 0; i < inventory.SlotCount; i++)
            {
                var slot = inventory.GetSlot(i);
                if (slot.IsEmpty) continue;
                sb.AppendLine($"{slot.Item.itemName} x{slot.Count}");
            }

            inventoryText.text = sb.Length > 0 ? sb.ToString() : "Envanter bos";
        }
    }
}