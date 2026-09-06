using UnityEngine;
using Project.Core;
using Project.Data;
using Project.Systems;
using Project.Gameplay.Interaction;
using Project.Gameplay.Player;

namespace Project.Gameplay.Items
{
    [RequireComponent(typeof(Collider))]
    public class LootContainer : MonoBehaviour, IInteractable
    {
        [SerializeField] private LootTableData lootTable;
        [Tooltip("Opsiyonel - acilinca gizlenecek/degisecek gorsel (ornegin kapali sandik modeli).")]
        [SerializeField] private GameObject visualToHideWhenLooted;

        private IInputProvider _input;
        private bool _playerInRange;
        private bool _hasBeenLooted;

        public string InteractionPrompt => _hasBeenLooted ? "Boþ" : "Ara";

        private void Start()
        {
            _input = ActiveInput.Provider;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) _playerInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) _playerInRange = false;
        }

        private void Update()
        {
            if (!_playerInRange || _input == null || _hasBeenLooted) return;

            if (_input.InteractPressedThisFrame)
            {
                Interact();
            }
        }

        public void Interact()
        {
            if (_hasBeenLooted) return;
            _hasBeenLooted = true;

            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            var loot = lootTable.Roll();

            if (loot.Count == 0)
            {
                Debug.Log("[LootContainer] Boþ çýktý.");
            }

            foreach (var (item, amount) in loot)
            {
                int leftover = inventory.AddItem(item, amount);
                int actuallyAdded = amount - leftover;
                if (actuallyAdded > 0)
                {
                    Debug.Log($"[LootContainer] Yaðmalandý: {item.itemName} x{actuallyAdded}");
                }
            }

            if (visualToHideWhenLooted != null)
            {
                visualToHideWhenLooted.SetActive(false);
            }
        }
    }
}