using UnityEngine;
using Project.Gameplay.Interaction;
using Project.Gameplay.Player;
using Project.UI;

namespace Project.Gameplay.Items
{
    [RequireComponent(typeof(Collider))]
    public class WaterSourceInteractable : MonoBehaviour, IInteractable
    {
        private IInputProvider _input;
        private bool _playerInRange;
        private EquipmentController _playerEquipment;

        public string InteractionPrompt
        {
            get
            {
                if (_playerEquipment == null || !_playerEquipment.IsBucketEquipped())
                    return "Once bos bir kova equip et";

                if (_playerEquipment.HasFullBucketEquipped())
                    return "Kova zaten dolu";

                return "Kovayi doldur (F)";
            }
        }

        private void Start()
        {
            _input = ActiveInput.Provider;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = true;
                _playerEquipment = other.GetComponent<EquipmentController>();
                if (_playerEquipment == null)
                {
                    _playerEquipment = other.GetComponentInParent<EquipmentController>();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = false;
                _playerEquipment = null;
                InteractionPromptUI.Instance?.Hide(gameObject);
            }
        }

        private void Update()
        {
            if (_playerInRange)
            {
                InteractionPromptUI.Instance?.Show(gameObject, InteractionPrompt);
            }

            if (!_playerInRange || _input == null) return;

            if (_input.InteractPressedThisFrame)
            {
                Interact();
            }
        }

        public void Interact()
        {
            if (_playerEquipment == null) return;

            bool filled = _playerEquipment.TryFillEquippedBucket();
            Debug.Log(filled
                ? "[WaterSourceInteractable] Kova dolduruldu."
                : "[WaterSourceInteractable] Kova dolu degil ya da equip edilmemis.");
        }
    }
}