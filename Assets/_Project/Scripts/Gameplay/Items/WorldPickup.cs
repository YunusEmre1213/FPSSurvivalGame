using UnityEngine;
using Project.Core;
using Project.Data;
using Project.Systems;
using Project.Gameplay.Interaction;
using Project.Gameplay.Player;

namespace Project.Gameplay.Items
{
    [RequireComponent(typeof(Collider))]
    public class WorldPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private WeaponPartData part;
        [SerializeField] private int amount = 1;

        private IInputProvider _input;
        private bool _playerInRange;

        public string InteractionPrompt => $"{part.partName} topla";

        private void Start()
        {
            _input = ActiveInput.Provider;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = false;
            }
        }

        private void Update()
        {
            if (!_playerInRange || _input == null) return;

            if (_input.PickupPressedThisFrame)
            {
                Interact();
            }
        }

        public void Interact()
        {
            var inventory = ServiceLocator.Instance.Get<IInventoryService>();
            inventory.AddPart(part, amount);

            int total = inventory.GetPartCount(part);
            Debug.Log($"[WorldPickup] Toplandý: {part.partName} x{amount} (toplam: {total})");

            Destroy(gameObject);
        }
    }
}