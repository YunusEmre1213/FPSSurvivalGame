using Project.Core;
using Project.Data;
using Project.Gameplay.Player;
using Project.Gameplay.Tools;
using Project.Gameplay.Weapons;
using Project.Systems;
using UnityEngine;

namespace Project.Gameplay.Items
{
    public class EquipmentController : MonoBehaviour
    {
        [SerializeField] private WeaponController weaponController;
        [SerializeField] private FlashlightController flashlightController;
        [Header("Balta")]
        [Tooltip("Envanterde bu esyadan en az 1 tane olmadan balta equip edilemez.")]
        [SerializeField] private AxeData axeItem;
        [SerializeField] private AxeController axeController;
        [Header("Kova")]
        [Tooltip("Envanterde bu esyadan en az 1 tane olmadan kova equip edilemez.")]
        [SerializeField] private BucketData bucketItem;
        [SerializeField] private BucketController bucketController;

        private enum EquippedItem { Weapon, Flashlight, Axe, Bucket }
        private EquippedItem _current = EquippedItem.Weapon;
        private IInputProvider _input;

        private void Start()
        {
            _input = ActiveInput.Provider;
            ApplyEquipped();
        }

        private void Update()
        {
            if (_input == null) return;

            if (_input.EquipWeaponPressedThisFrame && _current != EquippedItem.Weapon)
            {
                _current = EquippedItem.Weapon;
                ApplyEquipped();
            }
            else if (_input.EquipFlashlightPressedThisFrame && _current != EquippedItem.Flashlight)
            {
                _current = EquippedItem.Flashlight;
                ApplyEquipped();
            }
            else if (_input.EquipAxePressedThisFrame && _current != EquippedItem.Axe)
            {
                TryEquipAxe();
            }
            else if (_input.EquipBucketPressedThisFrame && _current != EquippedItem.Bucket)
            {
                TryEquipBucket();
            }
        }

        private void TryEquipAxe()
        {
            if (axeItem == null)
            {
                Debug.LogWarning("[EquipmentController] axeItem atanmamis - Inspector'dan AxeData surukle.");
                return;
            }

            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            if (inventory.GetItemCount(axeItem) <= 0)
            {
                Debug.Log("[EquipmentController] Balta yok - once bir balta bulman gerekiyor.");
                return;
            }

            _current = EquippedItem.Axe;
            ApplyEquipped();
        }

        private void TryEquipBucket()
        {
            if (bucketItem == null)
            {
                Debug.LogWarning("[EquipmentController] bucketItem atanmamis - Inspector'dan BucketData surukle.");
                return;
            }

            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            if (inventory.GetItemCount(bucketItem) <= 0)
            {
                Debug.Log("[EquipmentController] Kova yok - once bir kova bulman gerekiyor.");
                return;
            }

            _current = EquippedItem.Bucket;
            ApplyEquipped();
        }

        public bool TryFillEquippedBucket()
        {
            if (_current != EquippedItem.Bucket || bucketController == null) return false;
            return bucketController.TryFill();
        }

        public bool HasFullBucketEquipped()
        {
            return _current == EquippedItem.Bucket && bucketController != null && bucketController.IsFull;
        }

        public bool IsBucketEquipped()
        {
            return _current == EquippedItem.Bucket;
        }

        public void HideEquippedBucketForBoiling()
        {
            if (bucketController != null) bucketController.HideForBoiling();
        }

        public void ShowEquippedBucketAfterBoiling()
        {
            if (bucketController != null) bucketController.ShowAfterBoiling();
        }

        public void EmptyEquippedBucket()
        {
            if (bucketController != null) bucketController.Empty();
        }

        private void ApplyEquipped()
        {
            weaponController.SetEquipped(_current == EquippedItem.Weapon);
            flashlightController.SetEquipped(_current == EquippedItem.Flashlight);
            axeController.SetEquipped(_current == EquippedItem.Axe);
            bucketController.SetEquipped(_current == EquippedItem.Bucket);
            Debug.Log($"[EquipmentController] Elde: {_current}");
        }
    }
}