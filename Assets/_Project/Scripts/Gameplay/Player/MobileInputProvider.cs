using UnityEngine;

namespace Project.Gameplay.Player
{
    public class MobileInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] private UI.VirtualJoystick moveJoystick;
        [SerializeField] private UI.TouchLookArea lookArea;
        [Tooltip("Henüz ekranda ateþ butonu kurmadýysan boþ býrakabilirsin - FireHeld/FirePressedThisFrame o zaman hep false döner.")]
        [SerializeField] private UI.FireButton fireButton;

        public Vector2 MoveInput => moveJoystick.Value;

        public Vector2 LookDelta => lookArea.ConsumeDelta();

        public bool FireHeld => fireButton != null && fireButton.Held;

        public bool FirePressedThisFrame => fireButton != null && fireButton.ConsumePressedThisFrame();
    }
}