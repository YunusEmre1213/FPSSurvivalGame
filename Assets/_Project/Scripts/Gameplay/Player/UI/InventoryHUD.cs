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
            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
        }

        private void Start()
        {
            Refresh();
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            Refresh();
        }

        private void Refresh()
        {
            var inventory = ServiceLocator.Instance.Get<IInventoryService>();
            var sb = new StringBuilder();

            foreach (var kvp in inventory.GetAllParts())
            {
                if (kvp.Key == null) continue;
                sb.AppendLine($"{kvp.Key.partName} x{kvp.Value}");
            }

            inventoryText.text = sb.Length > 0 ? sb.ToString() : "Envanter bos";
        }
    }
}