using UnityEngine;
using Project.Core;

namespace Project.Gameplay.Interaction
{
    public class ObjectiveTarget : MonoBehaviour
    {
        [Tooltip("Ilgili ObjectiveData asset'indeki Target Id ile BIREBIR eslesmeli.")]
        [SerializeField] private string targetId;
        [Tooltip("Kapali birakirsan, sadece elle Trigger() cagrisiyla tetiklenir (InteractWithTarget kullanimi).")]
        [SerializeField] private bool triggerOnPlayerEnter = true;

        private bool _hasTriggered;

        private void OnTriggerEnter(Collider other)
        {
            if (!triggerOnPlayerEnter || _hasTriggered) return;
            if (other.CompareTag("Player"))
            {
                Trigger();
            }
        }
        public void Trigger()
        {
            if (_hasTriggered) return;

            _hasTriggered = true;
            EventBus.Publish(new ObjectiveTargetTriggeredEvent(targetId));
            Debug.Log($"[ObjectiveTarget] Tetiklendi: {targetId}");
        }
    }
}