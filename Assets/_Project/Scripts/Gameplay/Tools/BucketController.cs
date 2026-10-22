using UnityEngine;
using Project.Data;
using Project.Gameplay.Player;

namespace Project.Gameplay.Tools
{
    public class BucketController : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private Transform bucketSocket;
        [SerializeField] private BucketData bucketData;
        [SerializeField] private AudioSource audioSource;

        [Header("Dolu Gorseli")]
        [SerializeField] private string waterFillChildName = "WaterFill";

        [Header("Yuruyus Sallanmasi (Bob)")]
        [SerializeField] private float bobFrequency = 6f;
        [SerializeField] private float bobAmplitude = 0.02f;
        [SerializeField] private float sprintBobMultiplier = 1.6f;
        [SerializeField] private float blendSpeed = 5f;

        [Header("Yon Egilmesi (Lean)")]
        [SerializeField] private float strafeLeanAmount = 5f;
        [SerializeField] private float forwardLeanAmount = 2.5f;

        private GameObject _bucketModelInstance;
        private Transform _waterFillVisual;
        private bool _isEquipped;
        private bool _isFull;

        private Vector3 _basePosition;
        private Quaternion _baseRotation;
        private Vector3 _bobPositionOffset;
        private Vector3 _bobRotationOffset;
        private float _bobTimer;
        private float _bobBlend;

        private IInputProvider _input;

        public bool IsEquipped => _isEquipped;
        public bool IsFull => _isFull;

        /// Dolu kova equip edilmisken true - FirstPersonController hiz dususu icin buna bakar.
        public static bool IsCarryingFullBucket { get; private set; }

        private void Start()
        {
            _input = ActiveInput.Provider;
        }

        private void OnDestroy()
        {
            if (IsCarryingFullBucket) IsCarryingFullBucket = false;
        }

        private void Update()
        {
            if (!_isEquipped || _bucketModelInstance == null) return;

            UpdateBob();

            _bucketModelInstance.transform.localPosition = _basePosition + _bobPositionOffset;
            _bucketModelInstance.transform.localRotation = _baseRotation * Quaternion.Euler(_bobRotationOffset);
        }

        private void UpdateBob()
        {
            if (_input == null)
            {
                _bobPositionOffset = Vector3.zero;
                _bobRotationOffset = Vector3.zero;
                return;
            }

            Vector2 move = _input.MoveInput;
            float moveMagnitude = move.magnitude;
            float speedMultiplier = _input.SprintHeld ? sprintBobMultiplier : 1f;

            float targetBlend = moveMagnitude > 0.1f ? 1f : 0f;
            _bobBlend = Mathf.Lerp(_bobBlend, targetBlend, Time.deltaTime * blendSpeed);

            if (moveMagnitude > 0.1f)
            {
                _bobTimer += Time.deltaTime * bobFrequency * speedMultiplier;
            }

            float horizontal = Mathf.Sin(_bobTimer) * bobAmplitude;
            float vertical = Mathf.Sin(_bobTimer * 2f) * bobAmplitude * 0.5f;

            float strafeLean = -move.x * strafeLeanAmount;
            float forwardLean = move.y * forwardLeanAmount;

            _bobPositionOffset = new Vector3(horizontal, vertical, 0f) * _bobBlend;
            _bobRotationOffset = (new Vector3(vertical * 50f, 0f, horizontal * 70f + strafeLean) + new Vector3(forwardLean, 0f, 0f)) * _bobBlend;
        }

        public void SetEquipped(bool equipped)
        {
            _isEquipped = equipped;

            if (equipped)
            {
                SpawnBucketModel();
            }
            else if (_bucketModelInstance != null)
            {
                Destroy(_bucketModelInstance);
                _bucketModelInstance = null;
                _waterFillVisual = null;
            }

            RefreshCarryingFlag();
        }

        private void SpawnBucketModel()
        {
            if (_bucketModelInstance != null) Destroy(_bucketModelInstance);
            if (bucketData == null || bucketData.bucketModelPrefab == null || bucketSocket == null) return;

            _bucketModelInstance = Instantiate(bucketData.bucketModelPrefab, bucketSocket);
            _bucketModelInstance.transform.localPosition = Vector3.zero;
            _bucketModelInstance.transform.localRotation = Quaternion.identity;

            _basePosition = _bucketModelInstance.transform.localPosition;
            _baseRotation = _bucketModelInstance.transform.localRotation;

            var fillTransform = _bucketModelInstance.transform.Find(waterFillChildName);
            _waterFillVisual = fillTransform;
            UpdateFillVisual();
        }

        public bool TryFill()
        {
            if (!_isEquipped || _isFull) return false;

            _isFull = true;
            UpdateFillVisual();
            RefreshCarryingFlag();

            if (audioSource != null && bucketData != null && bucketData.fillSound != null)
            {
                audioSource.PlayOneShot(bucketData.fillSound);
            }

            Debug.Log("[BucketController] Kova dolduruldu.");
            return true;
        }

        public void Empty()
        {
            _isFull = false;
            UpdateFillVisual();
            RefreshCarryingFlag();
        }

        public void HideForBoiling()
        {
            if (_bucketModelInstance != null) _bucketModelInstance.SetActive(false);
        }

        public void ShowAfterBoiling()
        {
            if (_bucketModelInstance != null) _bucketModelInstance.SetActive(true);
        }

        private void UpdateFillVisual()
        {
            if (_waterFillVisual != null) _waterFillVisual.gameObject.SetActive(_isFull);
        }

        private void RefreshCarryingFlag()
        {
            IsCarryingFullBucket = _isEquipped && _isFull;
        }
    }
}