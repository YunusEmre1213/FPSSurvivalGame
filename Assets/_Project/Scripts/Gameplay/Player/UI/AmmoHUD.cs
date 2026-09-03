using UnityEngine;
using TMPro;
using Project.Core;
using Project.Data;
using Project.Systems;
using Project.Gameplay.Weapons;

namespace Project.UI
{
    public class AmmoHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private ResourceData ammoItem;

        private int _currentAmmo;
        private int _magazineCapacity;

        private void OnEnable()
        {
            EventBus.Subscribe<AmmoChangedEvent>(OnAmmoChanged);
            EventBus.Subscribe<InventorySlotChangedEvent>(OnInventoryChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AmmoChangedEvent>(OnAmmoChanged);
            EventBus.Unsubscribe<InventorySlotChangedEvent>(OnInventoryChanged);
        }

        private void OnAmmoChanged(AmmoChangedEvent evt)
        {
            _currentAmmo = evt.CurrentAmmo;
            _magazineCapacity = evt.MagazineCapacity;
            Refresh();
        }

        private void OnInventoryChanged(InventorySlotChangedEvent evt)
        {
            Refresh();
        }

        private void Refresh()
        {
            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            int reserve = inventory.GetItemCount(ammoItem);
            ammoText.text = $"{_currentAmmo} / {reserve}";
        }
    }
}