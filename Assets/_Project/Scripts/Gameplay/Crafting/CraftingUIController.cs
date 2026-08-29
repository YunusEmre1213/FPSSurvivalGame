using UnityEngine;
using UnityEngine.UI;
using Project.Core;
using Project.Data;
using Project.Systems;
using Project.Gameplay.Weapons;

namespace Project.Gameplay.Crafting
{
    public class CraftingUIController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;

        [Header("Silah verisi")]
        [SerializeField] private WeaponBaseData baseWeapon;

        [Header("Barrel secenekleri (iki secenekli - karsilikli dislama)")]
        [SerializeField] private WeaponPartData barrelOption;
        [SerializeField] private WeaponPartData barrelOptionAlt;

        [Header("Diger parcalar (slot basina tek secenek)")]
        [SerializeField] private WeaponPartData magazineOption;
        [SerializeField] private WeaponPartData stockOption;
        [SerializeField] private WeaponPartData sightOption;

        [Header("Slot butonlari")]
        [SerializeField] private Button barrelButton;
        [SerializeField] private Button barrelAltButton;
        [SerializeField] private Button magazineButton;
        [SerializeField] private Button stockButton;
        [SerializeField] private Button sightButton;

        [Header("Slot buton metinleri")]
        [SerializeField] private Text barrelButtonText;
        [SerializeField] private Text barrelAltButtonText;
        [SerializeField] private Text magazineButtonText;
        [SerializeField] private Text stockButtonText;
        [SerializeField] private Text sightButtonText;

        [Header("Onizleme ve onay")]
        [SerializeField] private Text statsPreviewText;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button closeButton;

        [Header("Hedef silah")]
        [SerializeField] private WeaponController targetWeaponController;

        private WeaponPartData _equippedBarrel; 
        private bool _magazineEquipped;
        private bool _stockEquipped;
        private bool _sightEquipped;

        private void Awake()
        {
            barrelButton.onClick.AddListener(() => ToggleBarrel(barrelOption));
            barrelAltButton.onClick.AddListener(() => ToggleBarrel(barrelOptionAlt));
            magazineButton.onClick.AddListener(() => ToggleSlot(WeaponPartType.Magazine));
            stockButton.onClick.AddListener(() => ToggleSlot(WeaponPartType.Stock));
            sightButton.onClick.AddListener(() => ToggleSlot(WeaponPartType.Sight));
            equipButton.onClick.AddListener(OnEquipPressed);
            closeButton.onClick.AddListener(Close);

            panelRoot.SetActive(false);
        }

        public void Open()
        {
            if (panelRoot.activeSelf) return;

            panelRoot.SetActive(true);
            UIInputLock.Lock();
            RefreshAll();
        }

        public void Close()
        {
            if (!panelRoot.activeSelf) return;

            panelRoot.SetActive(false);
            UIInputLock.Unlock();
        }

        private void ToggleBarrel(WeaponPartData option)
        {
            var inventory = ServiceLocator.Instance.Get<IInventoryService>();
            if (inventory.GetPartCount(option) <= 0) return;

            _equippedBarrel = (_equippedBarrel == option) ? null : option;
            RefreshAll();
        }

        private void ToggleSlot(WeaponPartType type)
        {
            var inventory = ServiceLocator.Instance.Get<IInventoryService>();

            switch (type)
            {
                case WeaponPartType.Magazine:
                    if (inventory.GetPartCount(magazineOption) > 0) _magazineEquipped = !_magazineEquipped;
                    break;
                case WeaponPartType.Stock:
                    if (inventory.GetPartCount(stockOption) > 0) _stockEquipped = !_stockEquipped;
                    break;
                case WeaponPartType.Sight:
                    if (inventory.GetPartCount(sightOption) > 0) _sightEquipped = !_sightEquipped;
                    break;
            }

            RefreshAll();
        }

        private void RefreshAll()
        {
            var inventory = ServiceLocator.Instance.Get<IInventoryService>();

            RefreshSlotButton(barrelButtonText, barrelOption, _equippedBarrel == barrelOption, inventory.GetPartCount(barrelOption));
            RefreshSlotButton(barrelAltButtonText, barrelOptionAlt, _equippedBarrel == barrelOptionAlt, inventory.GetPartCount(barrelOptionAlt));
            RefreshSlotButton(magazineButtonText, magazineOption, _magazineEquipped, inventory.GetPartCount(magazineOption));
            RefreshSlotButton(stockButtonText, stockOption, _stockEquipped, inventory.GetPartCount(stockOption));
            RefreshSlotButton(sightButtonText, sightOption, _sightEquipped, inventory.GetPartCount(sightOption));

            var previewAssembly = new WeaponAssembly(baseWeapon);
            if (_equippedBarrel != null) previewAssembly.EquipPart(_equippedBarrel);
            if (_magazineEquipped) previewAssembly.EquipPart(magazineOption);
            if (_stockEquipped) previewAssembly.EquipPart(stockOption);
            if (_sightEquipped) previewAssembly.EquipPart(sightOption);

            statsPreviewText.text = previewAssembly.CalculateStats().ToString();
        }

        private void RefreshSlotButton(Text label, WeaponPartData part, bool equipped, int ownedCount)
        {
            bool owned = ownedCount > 0;
            string riskTag = part.IsCorrupted ? $" [ARIZALI - RISK %{part.malfunctionChance * 100f:F0}]" : "";

            label.text = owned
                ? $"{part.partName} ({ownedCount}) - {(equipped ? "TAKILI" : "bos")}{riskTag}"
                : $"{part.partName} - sahip degilsin{riskTag}";

            label.color = part.IsCorrupted ? new Color(0.85f, 0.25f, 0.2f) : Color.white;
        }

        private void OnEquipPressed()
        {
            targetWeaponController.ApplyAssembly(
                _equippedBarrel,
                _magazineEquipped ? magazineOption : null,
                _stockEquipped ? stockOption : null,
                _sightEquipped ? sightOption : null);

            Debug.Log("[CraftingUIController] Silah guncellendi ve donatildi.");
            Close();
        }
    }
}