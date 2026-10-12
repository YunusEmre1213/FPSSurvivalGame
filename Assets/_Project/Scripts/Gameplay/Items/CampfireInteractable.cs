using System.Collections;
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

        [Header("Pisirme")]
        [SerializeField] private ConsumableData rawMeatItem;
        [SerializeField] private ConsumableData cookedMeatItem;
        [Tooltip("Bir etin pismesi icin gereken sure (saniye).")]
        [SerializeField] private float cookDuration = 8f;

        [Header("Gorsel referanslar (yanikken acik, sonukken kapali)")]
        [SerializeField] private GameObject fireVisual;
        [SerializeField] private GameObject fireLight;
        [Tooltip("Opsiyonel - varsa duman efekti de ayni sekilde ac/kapa.")]
        [SerializeField] private GameObject smokeVisual;

        private IInputProvider _input;
        private bool _playerInRange;
        private float _fuelRemaining;
        private bool _isCooking;

        public bool IsLit => _fuelRemaining > 0f;

        public string InteractionPrompt
        {
            get
            {
                if (!IsLit) return $"{woodItem.itemName} at (yakiti besle)";
                if (_isCooking) return "Et pisiyor...";
                return $"{rawMeatItem.itemName} pisir";
            }
        }

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
            if (!IsLit)
            {
                TryAddWood();
                return;
            }

            if (_isCooking)
            {
                Debug.Log("[CampfireInteractable] Et zaten pisiyor, bekle.");
                return;
            }

            if (TryStartCooking())
            {
                return;
            }

            // Pisirilecek et yoksa, ateþ yanýyorken de yine odun atilabilsin (yakiti besle)
            TryAddWood();
        }

        private void TryAddWood()
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

        private bool TryStartCooking()
        {
            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            if (inventory.GetItemCount(rawMeatItem) <= 0) return false;

            inventory.RemoveItem(rawMeatItem, 1);
            StartCoroutine(CookMeat());
            return true;
        }

        private IEnumerator CookMeat()
        {
            _isCooking = true;
            Debug.Log("[CampfireInteractable] Et pisirilmeye baslandi.");

            float elapsed = 0f;
            while (elapsed < cookDuration)
            {
                if (!IsLit)
                {
                    // Ates sonerse pisirme yarida kalir, et bosa gitmesin diye cig olarak iade edilir
                    var inv = ServiceLocator.Instance.Get<IItemInventoryService>();
                    inv.AddItem(rawMeatItem, 1);
                    _isCooking = false;
                    Debug.Log("[CampfireInteractable] Ates sondu, pisirme yarida kaldi - et geri verildi.");
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            inventory.AddItem(cookedMeatItem, 1);

            _isCooking = false;
            Debug.Log("[CampfireInteractable] Et pisti! Envantere eklendi.");
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