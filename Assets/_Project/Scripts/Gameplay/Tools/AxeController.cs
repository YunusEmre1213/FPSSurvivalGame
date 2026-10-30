using UnityEngine;
using Project.Data;
using Project.Gameplay.Player;
using Project.Gameplay.Resources;

namespace Project.Gameplay.Tools
{
    public class AxeController : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform axeSocket;
        [SerializeField] private AxeData axeData;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private HeldWeaponArmPose handPoseController;

        [Header("Elde Durus (Rest Pose)")]
        [Tooltip("Balta elde dururken (sallamadan once/sonra) socket'e gore yerel pozisyon offseti.")]
        [SerializeField] private Vector3 restPositionOffset;
        [Tooltip("Balta elde dururken socket'e gore yerel rotasyon offseti (derece).")]
        [SerializeField] private Vector3 restRotationOffset;

        [Header("Vurus Ayarlari")]
        [SerializeField] private float swingRange = 3f;
        [SerializeField] private float swingCooldown = 0.6f;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Sallama Animasyonu (Vurus)")]
        [SerializeField] private Vector3 swingPositionOffset = new Vector3(-0.1f, -0.15f, 0.3f);
        [SerializeField] private Vector3 swingRotationOffset = new Vector3(45f, -20f, -10f);
        [SerializeField] private float swingSpeed = 12f;
        [SerializeField] private float recoverySpeed = 6f;

        [Header("Yuruyus Sallanmasi (Bob)")]
        [SerializeField] private float bobFrequency = 6f;
        [SerializeField] private float bobAmplitude = 0.025f;
        [SerializeField] private float sprintBobMultiplier = 1.6f;
        [SerializeField] private float blendSpeed = 5f;

        [Header("Yon Egilmesi (Lean)")]
        [SerializeField] private float strafeLeanAmount = 6f;
        [SerializeField] private float forwardLeanAmount = 3f;

        private GameObject _axeModelInstance;
        private bool _isEquipped;
        private float _cooldownTimer;
        private bool _isSwinging;
        private float _swingProgress;
        private float _bobTimer;
        private float _bobBlend;

        private Vector3 _basePosition;
        private Quaternion _baseRotation;
        private Vector3 _currentPositionOffset;
        private Vector3 _currentRotationOffset;
        private Vector3 _bobPositionOffset;
        private Vector3 _bobRotationOffset;

        private IInputProvider _input;

        public bool IsEquipped => _isEquipped;

        private void Start()
        {
            _input = ActiveInput.Provider;
        }

        public void SetEquipped(bool equipped)
        {
            _isEquipped = equipped;

            if (equipped)
            {
                SpawnAxeModel();
            }
            else if (_axeModelInstance != null)
            {
                Destroy(_axeModelInstance);
                _axeModelInstance = null;
            }
        }

        private void SpawnAxeModel()
        {
            if (_axeModelInstance != null) Destroy(_axeModelInstance);
            if (axeData == null || axeData.axeModelPrefab == null || axeSocket == null) return;

            _axeModelInstance = Instantiate(axeData.axeModelPrefab, axeSocket);
            _axeModelInstance.transform.localPosition = restPositionOffset;
            _axeModelInstance.transform.localRotation = Quaternion.Euler(restRotationOffset);

            _basePosition = _axeModelInstance.transform.localPosition;
            _baseRotation = _axeModelInstance.transform.localRotation;
        }

        private void Update()
        {
            if (!_isEquipped) return;

            if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;

            UpdateAxeTransform();

            if (_input != null && _input.FirePressedThisFrame)
            {
                TrySwing();
            }
        }

        public void TrySwing()
        {
            if (!_isEquipped || _cooldownTimer > 0f) return;

            _cooldownTimer = swingCooldown;
            _isSwinging = true;
            _swingProgress = 0f;

            handPoseController?.TriggerFire();

            if (audioSource != null && axeData != null && axeData.swingSound != null)
            {
                audioSource.PlayOneShot(axeData.swingSound);
            }

            PerformHitDetection();
        }

        private void UpdateAxeTransform()
        {
            if (_axeModelInstance == null) return;

            if (_isSwinging)
            {
                _swingProgress += Time.deltaTime * swingSpeed;
                float t = Mathf.Sin(Mathf.Min(_swingProgress, 1f) * Mathf.PI);
                _currentPositionOffset = swingPositionOffset * t;
                _currentRotationOffset = swingRotationOffset * t;

                handPoseController?.SetSwingProgress(t);

                if (_swingProgress >= 1f) _isSwinging = false;
            }
            else
            {
                UpdateBob();

                _currentPositionOffset = Vector3.Lerp(_currentPositionOffset, _bobPositionOffset, recoverySpeed * Time.deltaTime);
                _currentRotationOffset = Vector3.Lerp(_currentRotationOffset, _bobRotationOffset, recoverySpeed * Time.deltaTime);

                handPoseController?.SetSwingProgress(0f);
            }

            _axeModelInstance.transform.localPosition = _basePosition + _currentPositionOffset;
            _axeModelInstance.transform.localRotation = _baseRotation * Quaternion.Euler(_currentRotationOffset);
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

        private void PerformHitDetection()
        {
            if (playerCamera == null) return;

            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, swingRange, hitMask))
            {
                TreeInteractable tree = hit.collider.GetComponent<TreeInteractable>();
                if (tree != null)
                {
                    int damage = axeData != null ? axeData.chopDamage : 1;
                    tree.ReceiveChop(damage);

                    if (audioSource != null && axeData != null && axeData.hitSound != null)
                    {
                        audioSource.PlayOneShot(axeData.hitSound);
                    }
                }
            }
        }
    }
}