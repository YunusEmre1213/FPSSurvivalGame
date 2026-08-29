using UnityEngine;
using Project.Core;
using Project.Systems;
using Project.Gameplay.Interaction;
using Project.Gameplay.Player;

namespace Project.Gameplay.Items
{

    [RequireComponent(typeof(Collider))]
    public class KeyItemPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string keyId;
        [SerializeField] private string displayName = "Anahtar Esya";

        private IInputProvider _input;
        private bool _playerInRange;

        public string InteractionPrompt => $"{displayName} topla";

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
            if (!_playerInRange || _input == null) return;

            if (_input.PickupPressedThisFrame)
            {
                Interact();
            }
        }

        public void Interact()
        {
            var keyItems = ServiceLocator.Instance.Get<IKeyItemService>();
            keyItems.Unlock(keyId);

            Debug.Log($"[KeyItemPickup] Kazanildi: {displayName} ({keyId})");
            Destroy(gameObject);
        }
    }
}