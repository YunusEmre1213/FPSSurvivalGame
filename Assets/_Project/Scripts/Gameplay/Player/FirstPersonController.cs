using UnityEngine;

namespace Project.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Bagimliliklar")]
        [Tooltip("IInputProvider implemente eden bilesen (MobileInputProvider veya KeyboardMouseInputProvider) buraya suruklenir.")]
        [SerializeField] private MonoBehaviour inputProviderSource;

        [Tooltip("Kameranin dikey (pitch) dondugu nokta - oyuncunun goz hizasinda bos bir child obje.")]
        [SerializeField] private Transform cameraPivot;

        [Header("Hareket")]
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float gravity = -18f;

        [Header("Nisan")]
        [SerializeField] private float lookSpeed = 3f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        private IInputProvider _input;
        private CharacterController _characterController;
        private float _verticalVelocity;
        private float _pitch;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            _input = inputProviderSource as IInputProvider;
            if (_input == null)
            {
                Debug.LogError("[FirstPersonController] inputProviderSource alani IInputProvider implemente etmiyor. " +
                                "Inspector'dan MobileInputProvider veya KeyboardMouseInputProvider surukle.");
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
            var lookDelta = _input.LookDelta;

            transform.Rotate(Vector3.up, lookDelta.x * lookSpeed);

            _pitch = Mathf.Clamp(_pitch - lookDelta.y * lookSpeed, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void HandleMove()
        {
            var moveInput = _input.MoveInput;
            var moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -1f; 
            }
            _verticalVelocity += gravity * Time.deltaTime;

            var velocity = moveDirection * moveSpeed;
            velocity.y = _verticalVelocity;

            _characterController.Move(velocity * Time.deltaTime);
        }
    }
}