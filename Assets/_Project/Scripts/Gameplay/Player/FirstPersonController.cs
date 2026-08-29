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

        [Header("Nisan")]
        [SerializeField] private float lookSpeed = 3f;
        [Tooltip("Dokunmatik girdideki titremeyi azaltir. 0 = hic yumusatma (en hizli tepki, en titrek). Yuksek deger = daha yumusak ama gecikmeli.")]
        [SerializeField] private float lookSmoothTime = 0.03f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        private IInputProvider _input;
        private CharacterController _characterController;

        private float _verticalVelocity;
        private float _pitch;

       
        private Vector3 _currentMoveVelocity;
        private Vector3 _moveVelocitySmoothRef;

      
        private Vector2 _smoothedLookDelta;
        private Vector2 _lookDeltaSmoothRef;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
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
            var targetVelocity = targetDirection * moveSpeed;

            _currentMoveVelocity = Vector3.SmoothDamp(_currentMoveVelocity, targetVelocity, ref _moveVelocitySmoothRef, moveSmoothTime);

            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -1f;
            }
            _verticalVelocity += gravity * Time.deltaTime;

            var finalVelocity = _currentMoveVelocity;
            finalVelocity.y = _verticalVelocity;

            _characterController.Move(finalVelocity * Time.deltaTime);
        }
    }
}