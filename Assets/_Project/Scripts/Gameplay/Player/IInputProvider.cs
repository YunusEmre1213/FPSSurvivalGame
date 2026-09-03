using UnityEngine;

namespace Project.Gameplay.Player
{
    public interface IInputProvider
    {
        Vector2 MoveInput { get; }
        Vector2 LookDelta { get; }
        bool FireHeld { get; }
        bool FirePressedThisFrame { get; }
        bool PickupPressedThisFrame { get; }
        bool InteractPressedThisFrame { get; }
        bool PausePressedThisFrame { get; }
        bool InventoryPressedThisFrame { get; }
        bool ReloadPressedThisFrame { get; }
    }
}