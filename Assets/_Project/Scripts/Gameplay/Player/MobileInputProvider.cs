using UnityEngine;

namespace Project.Gameplay.Player
{
    public class MobileInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] private UI.VirtualJoystick moveJoystick;
        [SerializeField] private UI.TouchLookArea lookArea;

        public Vector2 MoveInput => moveJoystick.Value;

        public Vector2 LookDelta => lookArea.ConsumeDelta();
    }
}