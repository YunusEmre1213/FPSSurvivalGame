using UnityEngine;
using Project.Core;

namespace Project.Gameplay.Player
{
    public class MobileInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] private UI.VirtualJoystick moveJoystick;
        [SerializeField] private UI.TouchLookArea lookArea;
        [SerializeField] private UI.HoldableButton fireButton;
        [SerializeField] private UI.HoldableButton pickupButton;
        [SerializeField] private UI.HoldableButton interactButton;

        public Vector2 MoveInput => UIInputLock.IsLocked ? Vector2.zero : moveJoystick.Value;

        public Vector2 LookDelta
        {
            get
            {
                var delta = lookArea.ConsumeDelta(); // her zaman drain et
                return UIInputLock.IsLocked ? Vector2.zero : delta;
            }
        }

        public bool FireHeld => !UIInputLock.IsLocked && fireButton != null && fireButton.Held;

        public bool FirePressedThisFrame
        {
            get
            {
                bool pressed = fireButton != null && fireButton.ConsumePressedThisFrame();
                return !UIInputLock.IsLocked && pressed;
            }
        }

        public bool PickupPressedThisFrame
        {
            get
            {
                bool pressed = pickupButton != null && pickupButton.ConsumePressedThisFrame();
                return !UIInputLock.IsLocked && pressed;
            }
        }

        public bool InteractPressedThisFrame
        {
            get
            {
                bool pressed = interactButton != null && interactButton.ConsumePressedThisFrame();
                return !UIInputLock.IsLocked && pressed;
            }
        }
    }
}