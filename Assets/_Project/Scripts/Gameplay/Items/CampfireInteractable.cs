using UnityEngine;
using Project.Core;
using Project.Data;
using Project.Systems;
using Project.Gameplay.Interaction;
using Project.Gameplay.Player;

namespace Project.Gameplay.Items
{
    [RequireComponent(typeof(Collider))]
    public class CampfireInteractable : MonoBehaviour, IInteractable
    {
        [Header("Yakit")]
        [SerializeField] private ResourceData woodItem;
        [Tooltip("Bir odunun ekledigi yakit suresi (saniye).")]
        [SerializeField] private float fuelPerWood = 60f;
        [Tooltip("Yakitin birikebilecegi ust sinir (saniye) - sonsuza kadar odun yigmayi onler.")]
        [SerializeField] private float maxFuel = 300f;

        [Header("Gorsel referanslar (yanikken acik, sonukken kapali)")]
        [SerializeField] private GameObject fireVisual;
        [SerializeField] private GameObject fireLight;
        [Tooltip("Opsiyonel - varsa duman efekti de ayni sekilde ac/kapa.")]
        [SerializeField] private GameObject smokeVisual;

        private IInputProvider _input;
        private bool _playerInRange;
        private float _fuelRemaining;

        public bool IsLit => _fuelRemaining > 0f;

        public string InteractionPrompt => IsLit ? $"{woodItem.itemName} at (yakiti besle)" : "Atesi yak";

        private void Start()
        {
            _input = ActiveInput.Provider;
            ApplyVisualState();
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
            if (IsLit)
            {
                _fuelRemaining -= Time.deltaTime;
                if (_fuelRemaining <= 0f)
                {
                    _fuelRemaining = 0f;
                    ApplyVisualState();
                    Debug.Log("[CampfireInteractable] Ates sondu - yakit bitti.");
                }
            }

            if (!_playerInRange || _input == null) return;

            if (_input.InteractPressedThisFrame)
            {
                Interact();
            }
        }

        public void Interact()
        {
            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            int removed = inventory.RemoveItem(woodItem, 1);

            if (removed <= 0)
            {
                Debug.Log($"[CampfireInteractable] {woodItem.itemName} yok - once toplaman gerekiyor.");
                return;
            }

            bool wasUnlit = !IsLit;
            _fuelRemaining = Mathf.Min(maxFuel, _fuelRemaining + fuelPerWood);

            if (wasUnlit)
            {
                ApplyVisualState();
                Debug.Log("[CampfireInteractable] Ates yakildi.");
            }
            else
            {
                Debug.Log($"[CampfireInteractable] Odun eklendi, kalan yakit: {_fuelRemaining:F0} saniye");
            }
        }

        private void ApplyVisualState()
        {
            bool lit = IsLit;
            if (fireVisual != null) fireVisual.SetActive(lit);
            if (fireLight != null) fireLight.SetActive(lit);
            if (smokeVisual != null) smokeVisual.SetActive(lit);
        }
    }
}