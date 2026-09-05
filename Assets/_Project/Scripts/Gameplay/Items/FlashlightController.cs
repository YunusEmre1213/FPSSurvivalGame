using UnityEngine;
using Project.Core;
using Project.Data;
using Project.Gameplay.Player;

namespace Project.Gameplay.Items
{
    public class FlashlightController : MonoBehaviour
    {
        [Header("Model")]
        [Tooltip("Kameraya bagli, fener modelinin yerlestirilecegi bos transform.")]
        [SerializeField] private Transform flashlightSocket;
        [SerializeField] private GameObject flashlightModelPrefab;
        [SerializeField] private Vector3 modelPositionOffset;
        [SerializeField] private Vector3 modelRotationOffset;

        [Header("Isik")]
        [Tooltip("Fener modelinin ICINDE, Light bileseninin oldugu objenin ADI.")]
        [SerializeField] private string lightPointName = "LightPoint";
        [Tooltip("Model icinde 'Light Point Name' bulunamazsa kullanilacak yedek Light referansi.")]
        [SerializeField] private Light fallbackLight;
        [SerializeField] private float maxIntensity = 8f;
        [SerializeField] private float minIntensity = 0.6f;

        [Header("Pil")]
        [SerializeField] private float maxBattery = 100f;
        [SerializeField] private float drainRate = 1.2f;

        [Header("Titreme (dusuk pilde artar)")]
        [SerializeField] private float flickerThreshold = 25f;
        [SerializeField] private float maxFlickerAmount = 3f;
        [SerializeField] private float flickerSpeed = 15f;

        [Header("Ses (opsiyonel)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip toggleSound;
        [SerializeField] private AudioClip lowBatteryCrackleSound;
        [SerializeField] private float crackleCheckInterval = 2f;

        [Header("Hareket sallanmasi (bob) - WeaponController ile ayni teknik")]
        [SerializeField] private float walkBobSpeed = 8f;
        [SerializeField] private float walkBobAmplitude = 0.015f;
        [SerializeField] private float sprintBobSpeed = 13f;
        [SerializeField] private float sprintBobAmplitude = 0.03f;
        [SerializeField] private float bobBlendSpeed = 6f;

        public float CurrentBattery { get; private set; }
        public bool IsOn { get; private set; }

        private IInputProvider _input;
        private float _flickerSeed;
        private float _crackleTimer;
        private bool _isEquipped;
        private GameObject _modelInstance;
        private Light _activeLight;
        private float _bobTimer;
        private float _bobAmplitude;
        private Vector3 _bobOffset;

        private void Awake()
        {
            CurrentBattery = 0f;
            _flickerSeed = Random.value * 100f;
            SpawnModel();
        }

        private void Start()
        {
            _input = ActiveInput.Provider;
        }

        private void SpawnModel()
        {
            if (flashlightSocket == null || flashlightModelPrefab == null) return;

            _modelInstance = Instantiate(flashlightModelPrefab, flashlightSocket);
            _modelInstance.transform.localPosition = modelPositionOffset;
            _modelInstance.transform.localRotation = Quaternion.Euler(modelRotationOffset);
            _modelInstance.SetActive(false);

            var found = FindDeepChild(_modelInstance.transform, lightPointName);
            _activeLight = found != null ? found.GetComponent<Light>() : fallbackLight;

            if (_activeLight == null)
            {
                Debug.LogWarning($"[FlashlightController] '{lightPointName}' adinda Light bulunamadi ve Fallback Light de bos - isik hic calismayacak.");
            }
            else
            {
                _activeLight.enabled = false;
            }
        }

        private Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;

                var found = FindDeepChild(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private void Update()
        {
            if (_isEquipped && _input != null && _input.FlashlightToggledThisFrame)
            {
                SetOn(!IsOn);
            }

            if (IsOn && CurrentBattery > 0f)
            {
                CurrentBattery = Mathf.Max(0f, CurrentBattery - drainRate * Time.deltaTime);
                UpdateLightIntensity();
                UpdateLowBatteryCrackle();

                if (CurrentBattery <= 0f)
                {
                    Debug.Log("[FlashlightController] Pil tukendi, fener sondu.");
                    SetOn(false);
                }
            }

            UpdateBob();
            ApplyModelTransform();
        }

        private void UpdateLightIntensity()
        {
            if (_activeLight == null) return;

            float batteryRatio = CurrentBattery / maxBattery;
            float baseIntensity = Mathf.Lerp(minIntensity, maxIntensity, batteryRatio);

            float flickerAmount = 0f;
            if (CurrentBattery < flickerThreshold)
            {
                float lowBatteryIntensity = 1f - (CurrentBattery / flickerThreshold);
                float noise = Mathf.PerlinNoise(_flickerSeed, Time.time * flickerSpeed);
                flickerAmount = (noise - 0.5f) * maxFlickerAmount * lowBatteryIntensity;
            }

            _activeLight.intensity = Mathf.Max(0f, baseIntensity + flickerAmount);
        }

        private void UpdateLowBatteryCrackle()
        {
            if (lowBatteryCrackleSound == null || audioSource == null || CurrentBattery >= flickerThreshold) return;

            _crackleTimer -= Time.deltaTime;
            if (_crackleTimer <= 0f)
            {
                float lowBatteryIntensity = 1f - (CurrentBattery / flickerThreshold);
                _crackleTimer = Mathf.Lerp(crackleCheckInterval, crackleCheckInterval * 0.2f, lowBatteryIntensity);
                audioSource.PlayOneShot(lowBatteryCrackleSound, 0.5f);
            }
        }
        private void UpdateBob()
        {
            if (_input == null) return;

            bool isMoving = _input.MoveInput.sqrMagnitude > 0.01f;
            bool isSprinting = isMoving && _input.SprintHeld;

            float targetSpeed = isSprinting ? sprintBobSpeed : walkBobSpeed;
            float targetAmplitude = isMoving ? (isSprinting ? sprintBobAmplitude : walkBobAmplitude) : 0f;

            _bobAmplitude = Mathf.Lerp(_bobAmplitude, targetAmplitude, bobBlendSpeed * Time.deltaTime);

            if (isMoving)
            {
                _bobTimer += Time.deltaTime * targetSpeed;
            }

            _bobOffset = new Vector3(
                Mathf.Sin(_bobTimer) * _bobAmplitude,
                Mathf.Sin(_bobTimer * 2f) * _bobAmplitude * 0.5f,
                0f);
        }

        private void ApplyModelTransform()
        {
            if (_modelInstance == null) return;

            _modelInstance.transform.localPosition = modelPositionOffset + _bobOffset;
        }

        private void SetOn(bool on)
        {
            if (on && CurrentBattery <= 0f)
            {
                Debug.Log("[FlashlightController] Pil yok - once bir pil bulup envanterden kullanman gerekiyor.");
            }

            IsOn = on && CurrentBattery > 0f && _isEquipped;

            if (_activeLight != null)
            {
                _activeLight.enabled = IsOn;
            }

            if (audioSource != null && toggleSound != null)
            {
                audioSource.PlayOneShot(toggleSound);
            }
        }
        public void SetEquipped(bool equipped)
        {
            _isEquipped = equipped;

            if (_modelInstance != null)
            {
                _modelInstance.SetActive(equipped);
            }
            IsOn = false;
            if (_activeLight != null)
            {
                _activeLight.enabled = false;
            }
        }

        public void Recharge(BatteryData battery)
        {
            CurrentBattery = Mathf.Min(maxBattery, CurrentBattery + battery.chargeRestore);
            Debug.Log($"[FlashlightController] Pil dolduruldu: {CurrentBattery:F0}/{maxBattery:F0}");
        }
    }
}