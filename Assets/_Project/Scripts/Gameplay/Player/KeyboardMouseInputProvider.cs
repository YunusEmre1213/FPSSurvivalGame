using UnityEngine;

namespace Project.Gameplay.Player
{
    public class KeyboardMouseInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] private float mouseSensitivity = 2f;

        public Vector2 MoveInput => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        public Vector2 LookDelta => new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity;

        public bool FireHeld => Input.GetMouseButton(0);

        public bool FirePressedThisFrame => Input.GetMouseButtonDown(0);
    }
}