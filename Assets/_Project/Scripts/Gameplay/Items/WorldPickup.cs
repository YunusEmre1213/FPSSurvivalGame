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
        [SerializeField] private ItemData item;
        [SerializeField] private int amount = 1;

        private IInputProvider _input;
        private bool _playerInRange;

        public string InteractionPrompt => $"{item.itemName} topla";

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
            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            int leftover = inventory.AddItem(item, amount);
            int actuallyAdded = amount - leftover;

            if (actuallyAdded > 0)
            {
                int total = inventory.GetItemCount(item);
                Debug.Log($"[WorldPickup] Toplandi: {item.itemName} x{actuallyAdded} (envanterde toplam: {total})");
            }

            if (leftover <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                amount = leftover;
                Debug.Log("[WorldPickup] Envanter dolu - kalan miktar objede kaldi, tekrar etkilesime gec.");
            }
        }
    }
}