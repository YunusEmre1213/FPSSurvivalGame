using Project.Core;
using Project.Gameplay.Player;
using Project.Gameplay.Weapons;
using UnityEngine;

namespace Project.Gameplay.Items
{
    public class EquipmentController : MonoBehaviour
    {
        [SerializeField] private WeaponController weaponController;
        [SerializeField] private FlashlightController flashlightController;

        private enum EquippedItem { Weapon, Flashlight }
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
        }

        private void ApplyEquipped()
        {
            weaponController.SetEquipped(_current == EquippedItem.Weapon);
            flashlightController.SetEquipped(_current == EquippedItem.Flashlight);
            Debug.Log($"[EquipmentController] Elde: {_current}");
        }
    }
}