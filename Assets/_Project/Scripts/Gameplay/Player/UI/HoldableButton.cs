using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.Gameplay.Player.UI
{
    public class HoldableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool Held { get; private set; }

        private bool _pressedThisFrame;

        public void OnPointerDown(PointerEventData eventData)
        {
            Held = true;
            _pressedThisFrame = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Held = false;
        }
        public bool ConsumePressedThisFrame()
        {
            var value = _pressedThisFrame;
            _pressedThisFrame = false;
            return value;
        }
    }
}