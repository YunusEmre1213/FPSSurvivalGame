using UnityEngine;
using Project.Core;

namespace Project.Gameplay.Player
{
    public class KeyboardMouseInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] private float mouseSensitivity = 2f;

        public Vector2 MoveInput => UIInputLock.IsLocked
            ? Vector2.zero
            : new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        public Vector2 LookDelta => UIInputLock.IsLocked
            ? Vector2.zero
            : new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity;

        public bool FireHeld => !UIInputLock.IsLocked && Input.GetMouseButton(0);

        public bool FirePressedThisFrame => !UIInputLock.IsLocked && Input.GetMouseButtonDown(0);

        public bool PickupPressedThisFrame => !UIInputLock.IsLocked && Input.GetKeyDown(KeyCode.E);

        public bool InteractPressedThisFrame => !UIInputLock.IsLocked && Input.GetKeyDown(KeyCode.F);

        public bool PausePressedThisFrame => Input.GetKeyDown(KeyCode.Escape);

        private void Awake()
        {
            ActiveInput.SetProvider(this);
        }

        private void Update()
        {
            if (UIInputLock.IsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}