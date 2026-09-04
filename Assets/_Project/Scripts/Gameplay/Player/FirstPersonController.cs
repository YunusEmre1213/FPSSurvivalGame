using UnityEngine;

namespace Project.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Bagimliliklar")]
        [Tooltip("Kameranin dikey (pitch) dondugu nokta - oyuncunun goz hizasinda bos bir child obje.")]
        [SerializeField] private Transform cameraPivot;

        [Header("Hareket")]
        [SerializeField] private float moveSpeed = 4.5f;
        [Tooltip("Hedef hiza ulasma suresi (saniye). Kucuk deger = ani baslama/durma, buyuk deger = kaygan/gecikmeli his.")]
        [SerializeField] private float moveSmoothTime = 0.08f;
        [SerializeField] private float gravity = -18f;

        [Header("Kosma (Shift)")]
        [SerializeField] private float sprintSpeedMultiplier = 1.8f;

        [Header("Ziplama (Space)")]
        [SerializeField] private float jumpHeight = 1.2f;

        [Header("Nisan")]
        [SerializeField] private float lookSpeed = 3f;
        [Tooltip("Dokunmatik girdideki titremeyi azaltir. 0 = hic yumusatma (en hizli tepki, en titrek). Yuksek deger = daha yumusak ama gecikmeli.")]
        [SerializeField] private float lookSmoothTime = 0.03f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        [Header("Adim sesi")]
        [SerializeField] private AudioSource footstepAudioSource;
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip landSound;
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float sprintStepInterval = 0.32f;

        private IInputProvider _input;
        private CharacterController _characterController;
        private PlayerStamina _stamina;

        private float _verticalVelocity;
        private float _pitch;
        private bool _isSprinting;
        private bool _wasGrounded = true;
        private float _footstepTimer;

       
        private Vector3 _currentMoveVelocity;
        private Vector3 _moveVelocitySmoothRef;

      
        private Vector2 _smoothedLookDelta;
        private Vector2 _lookDeltaSmoothRef;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _stamina = GetComponent<PlayerStamina>();
        }

        private void Start()
        {
            _input = ActiveInput.Provider;
            if (_input == null)
            {
                Debug.LogError("[FirstPersonController] ActiveInput.Provider hala null - sahnede InputSourceSelector var mi kontrol et.");
            }
        }

        private void Update()
        {
            if (_input == null) return;

            HandleLook();
            HandleMove();
        }

        private void HandleLook()
        {
            var rawDelta = _input.LookDelta;

            _smoothedLookDelta = Vector2.SmoothDamp(_smoothedLookDelta, rawDelta, ref _lookDeltaSmoothRef, lookSmoothTime);

            transform.Rotate(Vector3.up, _smoothedLookDelta.x * lookSpeed);

            _pitch = Mathf.Clamp(_pitch - _smoothedLookDelta.y * lookSpeed, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void HandleMove()
        {
            var moveInput = _input.MoveInput;
            var targetDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            targetDirection = Vector3.ClampMagnitude(targetDirection, 1f);

            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            bool wantsToSprint = _input.SprintHeld && isMoving;

            float currentSpeed = moveSpeed;
            _isSprinting = false;

            if (wantsToSprint && _stamina != null && _stamina.TryDrainForSprint(Time.deltaTime))
            {
                currentSpeed = moveSpeed * sprintSpeedMultiplier;
                _isSprinting = true;
            }
            else if (_stamina != null)
            {
                _stamina.Regenerate(Time.deltaTime);
            }

            var targetVelocity = targetDirection * currentSpeed;
            _currentMoveVelocity = Vector3.SmoothDamp(_currentMoveVelocity, targetVelocity, ref _moveVelocitySmoothRef, moveSmoothTime);

            bool isGrounded = _characterController.isGrounded;

            if (isGrounded && !_wasGrounded)
            {
                PlayLandSound();
            }
            _wasGrounded = isGrounded;

            if (isGrounded)
            {
                if (_verticalVelocity < 0f)
                {
                    _verticalVelocity = -1f;
                }

                if (_input.JumpPressedThisFrame)
                {
                    _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    PlayJumpSound();
                }
            }
            _verticalVelocity += gravity * Time.deltaTime;

            var finalVelocity = _currentMoveVelocity;
            finalVelocity.y = _verticalVelocity;

            _characterController.Move(finalVelocity * Time.deltaTime);

            HandleFootsteps(isMoving);
        }

        private void HandleFootsteps(bool isMoving)
        {
            if (!_characterController.isGrounded || !isMoving)
            {
                _footstepTimer = 0f;
                return;
            }

            _footstepTimer += Time.deltaTime;
            float interval = _isSprinting ? sprintStepInterval : walkStepInterval;

            if (_footstepTimer >= interval)
            {
                _footstepTimer = 0f;
                PlayFootstep();
            }
        }

        private void PlayFootstep()
        {
            if (footstepAudioSource == null || footstepClips == null || footstepClips.Length == 0) return;

            var clip = footstepClips[Random.Range(0, footstepClips.Length)];
            footstepAudioSource.PlayOneShot(clip);
        }

        private void PlayJumpSound()
        {
            if (footstepAudioSource != null && jumpSound != null)
            {
                footstepAudioSource.PlayOneShot(jumpSound);
            }
        }

        private void PlayLandSound()
        {
            if (footstepAudioSource != null && landSound != null)
            {
                footstepAudioSource.PlayOneShot(landSound);
            }
        }
    }
}