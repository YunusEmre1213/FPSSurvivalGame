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
        [SerializeField] private TMP_Text currentAmmoText;
        [SerializeField] private TMP_Text spareMagazinesText;
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
            int reserveRounds = inventory.GetItemCount(ammoItem);

            int spareMagazines = _magazineCapacity > 0 ? reserveRounds / _magazineCapacity : 0;

            currentAmmoText.text = _currentAmmo.ToString();
            spareMagazinesText.text = spareMagazines.ToString();
        }
    }
}