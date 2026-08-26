using UnityEngine;
using Project.Gameplay.Interaction;
using Project.Gameplay.Player;
using Project.Gameplay.Crafting;

namespace Project.Gameplay.Items
{

    [RequireComponent(typeof(Collider))]
    public class WorkbenchInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private MonoBehaviour inputProviderSource;
        [SerializeField] private CraftingUIController craftingUI;

        private IInputProvider _input;
        private bool _playerInRange;

        public string InteractionPrompt => "Workbench'i ac";

        private void Awake()
        {
            _input = inputProviderSource as IInputProvider;
            if (_input == null)
            {
                Debug.LogError("[WorkbenchInteractable] inputProviderSource alaný IInputProvider implemente etmiyor.");
            }
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

            if (_input.InteractPressedThisFrame)
            {
                Interact();
            }
        }

        public void Interact()
        {
            craftingUI.Open();
        }
    }
}