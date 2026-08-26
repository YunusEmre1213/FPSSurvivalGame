using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.Gameplay.Player.UI
{
    public class TouchLookArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private float sensitivity = 0.15f;

        private Vector2 _pendingDelta;

        public void OnPointerDown(PointerEventData eventData)
        {
            _pendingDelta = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _pendingDelta += eventData.delta * sensitivity;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pendingDelta = Vector2.zero;
        }
        public Vector2 ConsumeDelta()
        {
            var value = _pendingDelta;
            _pendingDelta = Vector2.zero;
            return value;
        }
    }
}