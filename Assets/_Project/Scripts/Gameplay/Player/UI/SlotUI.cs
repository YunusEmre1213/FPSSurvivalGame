using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Systems;

namespace Project.UI
{
    [RequireComponent(typeof(Button))]
    public class SlotUI : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text countText;

        [SerializeField, Range(0f, 1f)] private float emptyBackgroundAlpha = 0.55f;
        [SerializeField, Range(0f, 1f)] private float filledBackgroundAlpha = 0.9f;

        public Button Button { get; private set; }

        private void Awake()
        {
            Button = GetComponent<Button>();
        }

        public void Refresh(InventorySlot slot)
        {
            bool isEmpty = slot == null || slot.IsEmpty;

            SetBackgroundAlpha(isEmpty ? emptyBackgroundAlpha : filledBackgroundAlpha);

            if (isEmpty)
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

        private void SetBackgroundAlpha(float alpha)
        {
            if (backgroundImage == null) return;
            var c = backgroundImage.color;
            c.a = alpha;
            backgroundImage.color = c;
        }
    }
}