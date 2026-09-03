using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Systems;

namespace Project.UI
{
    public class SlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text countText;

        public void Refresh(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                iconImage.enabled = false;
                nameText.text = "";
                countText.text = "";
                return;
            }

            bool hasIcon = slot.Item.icon != null;
            iconImage.enabled = hasIcon;
            if (hasIcon)
            {
                iconImage.sprite = slot.Item.icon;
            }

            nameText.text = slot.Item.itemName;
            countText.text = slot.Count > 1 ? $"x{slot.Count}" : "";
        }
    }
}