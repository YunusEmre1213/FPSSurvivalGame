using UnityEngine;

namespace Project.Gameplay.Player
{
    public interface IInputProvider
    {
        Vector2 MoveInput { get; }

        Vector2 LookDelta { get; }
    }
}