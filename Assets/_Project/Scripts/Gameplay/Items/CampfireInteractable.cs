using System.Collections;
using UnityEngine;
using Project.Core;
using Project.Data;
using Project.Systems;
using Project.Gameplay.Interaction;
using Project.Gameplay.Player;
using Project.UI;

namespace Project.Gameplay.Items
{
    [RequireComponent(typeof(Collider))]
    public class CampfireInteractable : MonoBehaviour, IInteractable
    {
        [Header("Yakit")]
        [SerializeField] private ResourceData woodItem;
        [SerializeField] private float fuelPerWood = 60f;
        [SerializeField] private float maxFuel = 300f;

        [Header("Pisirme")]
        [SerializeField] private ConsumableData rawMeatItem;
        [SerializeField] private ConsumableData cookedMeatItem;
        [SerializeField] private float cookDuration = 8f;

        [Header("Su Kaynatma (Kova)")]
        [SerializeField] private ConsumableData cleanWaterItem;
        [SerializeField] private int bottlesPerBucket = 3;
        [SerializeField] private float boilDuration = 10f;
        [SerializeField] private Transform potSocket;
        [SerializeField] private GameObject boilingBucketVisualPrefab;

        [Header("Gorsel referanslar")]
        [SerializeField] private GameObject fireVisual;
        [SerializeField] private GameObject fireLight;
        [SerializeField] private GameObject smokeVisual;

        [Header("UI")]
        [SerializeField] private CampfirePanelUI campfirePanelUI;

        private IInputProvider _input;
        private bool _playerInRange;
        private float _fuelRemaining;
        private bool _isCooking;
        private bool _isBoiling;
        private EquipmentController _playerEquipment;
        private GameObject _boilingBucketVisualInstance;

        public bool IsLit => _fuelRemaining > 0f;
        public bool IsBusy => _isCooking || _isBoiling;

        public string InteractionPrompt
        {
            get
            {
                if (!IsLit) return $"{woodItem.itemName} at (yakiti besle)";
                return "Atesi kullan (F)";
            }
        }

        public string RawMeatLabel => rawMeatItem != null ? $"{rawMeatItem.itemName} Pisir" : "Et Pisir";
        public string BoilLabel => "Su Kaynat";
        public string WoodLabel => woodItem != null ? $"{woodItem.itemName} Ekle" : "Odun Ekle";

        public string StatusLabel
        {
            get
            {
                if (_isCooking) return "Et pisiyor...";
                if (_isBoiling) return "Su kayniyor...";
                return null;
            }
        }

        private void Start()
        {
            _input = ActiveInput.Provider;
            ApplyVisualState();
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
                campfirePanelUI?.CloseIfOwner(this);
            }
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
            if (!IsLit)
            {
                TryAddWood();
                return;
            }

            if (campfirePanelUI != null)
            {
                campfirePanelUI.Open(this);
            }
            else
            {
                Debug.LogWarning("[CampfireInteractable] campfirePanelUI atanmamis - panel acilamiyor.");
            }
        }

        public bool HasRawMeat()
        {
            return ServiceLocator.Instance.Get<IItemInventoryService>().GetItemCount(rawMeatItem) > 0;
        }

        public bool HasWood()
        {
            return ServiceLocator.Instance.Get<IItemInventoryService>().GetItemCount(woodItem) > 0;
        }

        public bool HasFullBucketReady()
        {
            return _playerEquipment != null && _playerEquipment.HasFullBucketEquipped();
        }

        public void StartCookingFromPanel()
        {
            TryStartCooking();
        }

        public void StartBoilingFromPanel()
        {
            TryStartBoilingWithBucket();
        }

        public void AddWoodFromPanel()
        {
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
            if (_isCooking || _isBoiling) return false;

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

        private bool TryStartBoilingWithBucket()
        {
            if (_isCooking || _isBoiling) return false;
            if (_playerEquipment == null || !_playerEquipment.HasFullBucketEquipped()) return false;

            var boilingPlayer = _playerEquipment;
            boilingPlayer.HideEquippedBucketForBoiling();
            SpawnBoilingBucketVisual();

            StartCoroutine(BoilBucket(boilingPlayer));
            return true;
        }

        private IEnumerator BoilBucket(EquipmentController boilingPlayer)
        {
            _isBoiling = true;
            Debug.Log("[CampfireInteractable] Kova atese kondu, su kaynatiliyor.");

            float elapsed = 0f;
            while (elapsed < boilDuration)
            {
                if (!IsLit)
                {
                    DespawnBoilingBucketVisual();
                    boilingPlayer.ShowEquippedBucketAfterBoiling();
                    _isBoiling = false;
                    Debug.Log("[CampfireInteractable] Ates sondu, kaynatma yarida kaldi - kova dolu haliyle geri verildi.");
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            DespawnBoilingBucketVisual();
            boilingPlayer.EmptyEquippedBucket();
            boilingPlayer.ShowEquippedBucketAfterBoiling();

            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            inventory.AddItem(cleanWaterItem, bottlesPerBucket);

            _isBoiling = false;
            Debug.Log($"[CampfireInteractable] Su kaynadi! Envantere {bottlesPerBucket}x {cleanWaterItem.itemName} eklendi.");
        }

        private void SpawnBoilingBucketVisual()
        {
            if (boilingBucketVisualPrefab == null || potSocket == null) return;
            if (_boilingBucketVisualInstance != null) Destroy(_boilingBucketVisualInstance);

            _boilingBucketVisualInstance = Instantiate(boilingBucketVisualPrefab, potSocket);
            _boilingBucketVisualInstance.transform.localPosition = Vector3.zero;
            _boilingBucketVisualInstance.transform.localRotation = Quaternion.identity;
        }

        private void DespawnBoilingBucketVisual()
        {
            if (_boilingBucketVisualInstance != null)
            {
                Destroy(_boilingBucketVisualInstance);
                _boilingBucketVisualInstance = null;
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