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

        private enum EquippedItem { Weapon, Flashlight, Axe }
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

        private void ApplyEquipped()
        {
            weaponController.SetEquipped(_current == EquippedItem.Weapon);
            flashlightController.SetEquipped(_current == EquippedItem.Flashlight);
            axeController.SetEquipped(_current == EquippedItem.Axe);
            Debug.Log($"[EquipmentController] Elde: {_current}");
        }
    }
}